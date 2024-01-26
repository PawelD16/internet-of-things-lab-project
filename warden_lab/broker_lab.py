import paho.mqtt.client as mqtt
import threading
from encryption_utils_lab import decode_pub_key, encode_message
from utilities_lab import is_room_number_valid
from mfrc522 import MFRC522

from config import *

from buzzer import buzzer

from menu import init_menu, menu, display_brightness, unauthorized_ui

broker_address = "10.108.33.121"

client = mqtt.Client()

authorized = False
can_read = True
answer = ""

current_room = 0

last_room_pointer = -1
last_brightness = -1
brightness = 0
encoder_topic = "messages/encoder"
keys_requests_topic = "messages/key/requests"
keys_answers_topic = "messages/key/answers"
brightness_status_topic = "messages/brightness"

server_command_topic = "server/command"
server_result_topic = "server/result"

current_pub_key = ""

allOptions = []

encoderLeftPrevoiusState = GPIO.input(encoderLeft)
encoderRightPrevoiusState = GPIO.input(encoderRight)

disp = None



def send_command_to_server(command):
    # example "rooms:uid" -> List<Room>
    client.publish(server_command_topic, payload=str(command))


def listen_for_rfid():
    global authorized
    global can_read
    MIFAREReader = MFRC522()
    (status, TagType) = MIFAREReader.MFRC522_Request(MIFAREReader.PICC_REQIDL)
    if status == MIFAREReader.MI_OK and can_read:
        (status, uid) = MIFAREReader.MFRC522_Anticoll()
        if status == MIFAREReader.MI_OK:
            num = 0
            for i in range(0, len(uid)):
                num += uid[i] << (i*8)
            can_read = False
            send_command_to_server(f"rooms:{num}")
            print(num)
            authorized = True
            timeout_thread = threading.Timer(0.4, check_for_rooms_answer)
            timeout_thread.start()
    else:
        pass


def check_for_rooms_answer():
    global can_read
    global authorized
    global answer
    if answer != "":
        can_read = False
        authorized = True
    else:
        buzzer(1)
        timeout_thread = threading.Timer(0.2, lambda: disable_buzzer())
        timeout_thread.start()
        authorized = False


def disable_buzzer():
    global can_read
    can_read = True
    buzzer(0)


def handle_brightness_data():
    global brightness
    global last_brightness
    if brightness != last_brightness:
        display_brightness(disp, brightness)
        last_brightness = brightness


def discard_room(e):
    global current_pub_key
    global current_room
    global last_room_pointer
    global last_brightness
    if authorized:
        if current_pub_key != "":
            current_room = 0
            last_room_pointer = -1
            last_brightness = -1
            current_pub_key = ""
        else:
            logout()


def setup_current_public_key(msg):
    global current_pub_key
    key_str = msg.payload.decode()
    key = decode_pub_key(key_str)
    current_pub_key = key


def on_message_received(c, userdata, msg):
    topic = msg.topic
    global answer
    global allOptions
    global brightness
    if topic == keys_answers_topic and msg.payload.decode() != "":
        setup_current_public_key(msg)
    elif topic == brightness_status_topic:
        brightness = msg.payload.decode()
    elif topic == server_result_topic:
        print(f"Response from server: {msg.payload.decode()}")
        answer = msg.payload.decode()
        allOptions = parse_rooms_answer(msg.payload.decode())


def rotation_decode(e):
    encoderLeftCurrentState = GPIO.input(encoderLeft)
    encoderRightCurrentState = GPIO.input(encoderRight)

    global encoderLeftPrevoiusState
    global encoderRightPrevoiusState
    global allOptions

    if (
        encoderLeftPrevoiusState == 1
        and encoderLeftCurrentState == 0
        and encoderRightCurrentState == 0
        and encoderRightPrevoiusState == 0
    ):
        # prawo
        message = str(0)
        if current_pub_key != "":
            encoded_message = encode_message(message, current_pub_key)
            client.publish(topic=encoder_topic, payload=encoded_message)
        if authorized:
            allOptions = allOptions[1:] + [allOptions[0]]

    if (
        encoderRightPrevoiusState == 1
        and encoderRightCurrentState == 0
        and encoderLeftPrevoiusState == 0
        and encoderLeftCurrentState == 0
    ):
        # lewo
        message = str(1)
        if current_pub_key != "":
            encoded_message = encode_message(message, current_pub_key)
            client.publish(topic=encoder_topic, payload=encoded_message)
        if authorized:
            allOptions = [allOptions[-1]] + allOptions[:-1]

    encoderLeftPrevoiusState = encoderLeftCurrentState
    encoderRightPrevoiusState = encoderRightCurrentState


def check_for_key_answer():
    if current_pub_key == "":
        buzzer(1)
        timeout_thread = threading.Timer(0.4, disable_buzzer)
        timeout_thread.start()


def display_available_rooms():
    global current_pub_key
    global last_room_pointer
    global allOptions
    if len(allOptions) > 0 and last_room_pointer != allOptions[0]:
        menu(disp, allOptions)
        last_room_pointer = allOptions[0]


def choose_room(e):
    global current_room
    if current_pub_key == "":
        current_room = int(allOptions[0])
    client.publish(topic=keys_requests_topic, payload=str(current_room))
    timeout_thread = threading.Timer(0.1, check_for_key_answer)
    timeout_thread.start()
    pass


def bind_controllers():
    GPIO.add_event_detect(
        encoderRight, GPIO.FALLING, callback=rotation_decode, bouncetime=20
    )
    GPIO.add_event_detect(
        encoderLeft, GPIO.FALLING, callback=rotation_decode, bouncetime=20
    )
    GPIO.add_event_detect(buttonRed, GPIO.FALLING, callback=discard_room, bouncetime=200)
    GPIO.add_event_detect(buttonGreen, GPIO.FALLING, callback=choose_room, bouncetime=200)


def subscribe():
    client.subscribe(keys_answers_topic)
    client.subscribe(brightness_status_topic)
    client.subscribe(server_result_topic)


def setup_broker():
    client.connect(broker_address, keepalive=0, port=1883)

    subscribe()

    client.on_message = on_message_received

    bind_controllers()
    client.loop_start()



def parse_rooms_answer(answer):
    split = answer.split(',')
    temp = []
    for item in split:
        if item != '':
            temp.append(int(item))
    return temp


def setup_disp():
    global disp
    disp = init_menu()


def clean_disp():
    global disp
    disp.fill(0)


def logout():
    global answer
    global can_read
    global authorized
    global allOptions
    global last_room_pointer
    answer = ""
    can_read = True
    authorized = False
    last_room_pointer = -1
    timeout_thread = threading.Timer(0.4, lambda : unauthorized_ui(disp))
    timeout_thread.start()


if __name__ == "__main__":
    setup_disp()
    setup_broker()
    while True:
        if not authorized:
            listen_for_rfid()
        elif authorized and current_pub_key == "":
            display_available_rooms()
        elif authorized and current_pub_key != "":
            handle_brightness_data()
