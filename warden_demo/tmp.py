from glob import glob
import paho.mqtt.client as mqtt
import keyboard
import threading
from encryption_utils_lab import decode_pub_key, encode_message
from utilities_lab import is_room_number_valid
from mfrc522 import MFRC522

from config import *

from buzzer import buzzer

from menu import init_menu, menu, display_brightness, reset

encoder_topic = "messages/encoder"
keys_requests_topic = "messages/key/requests"
keys_answers_topic = "messages/key/answers"
brightness_status_topic = "messages/brightness"

server_command_topic = "server/command"
server_rooms_result_topic = "server/result"

broker_address = "10.108.33.122"
def subscribe():
    client.subscribe(keys_answers_topic)
    client.subscribe(brightness_status_topic)
    client.subscribe(server_rooms_result_topic)
client = mqtt.Client()
client.connect(broker_address, keepalive=0, port=1883)
subscribe()

def on_message_received(c, userdata, msg):
    global answer
    topic = msg.topic
    print(msg.payload.decode())
    if topic == keys_answers_topic and msg.payload.decode() != "":
        setup_current_public_key(msg)
    elif topic == brightness_status_topic:
        brightness = msg.payload.decode()
        handle_brightness_data(brightness)
    elif topic == server_rooms_result_topic:
        answer = msg.payload.decode()
        print(f"Response from server: {answer}")

client.on_message = on_message_received

can_read = True
authorized = False

current_room = 0


current_pub_key = ""


answer = ""
allOptions = [1, 4, 7, 9]

encoderLeftPrevoiusState = GPIO.input(encoderLeft)
encoderRightPrevoiusState = GPIO.input(encoderRight)

disp = None

def logout():
    global answer
    global can_read
    global authorized
    answer = ""
    can_read = True
    authorized = False
    unauthorized_ui()


def disable_buzzer():
    global can_read
    can_read = True
    buzzer(0)


def check_for_rooms_answer():
    global can_read
    global authorized
    if answer != "":
        can_read = False
        authorized = True
        print(answer)
        # parse to rooms
        # display_available_rooms()
    else:
        buzzer(1)
        timeout_thread = threading.Timer(0.2, lambda: disable_buzzer())
        timeout_thread.start()
        authorized = False



def send_command_to_server(command):
    # example "rooms:uid" -> List<Room>
    client.publish(server_command_topic, payload=str(command))


def unauthorized_ui():
    global authorized
    global can_read
    while not authorized:
    # dopoki unauthorized -> czytaj karte
    # on authorization -> send_command_to_server(f"rooms:{uid}")
    # wait for response, convert to list.
    # if list is empty -> buzzer! else authorized_ui(list)
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
                timeout_thread = threading.Timer(0.4, check_for_rooms_answer)
                timeout_thread.start()

        else:
            authorized = False
            pass


def quick_buzzer():
    #threadami
    pass

def authorized_ui(rooms):
    pass


def management_ui():
    print(f"Managing room {current_room}")
    pass


def ask_for_rooms():
    pass


def handle_brightness_data(brightness):
    display_brightness(disp, brightness)
    pass


def discard_room(e):
    global current_pub_key
    global current_room
    if current_pub_key != "":
        print(f"Exiting room {current_room}")
        current_room = 0
        display_available_rooms()


def setup_current_public_key(msg):
    global current_pub_key
    key_str = msg.payload.decode()
    key = decode_pub_key(key_str)
    current_pub_key = key




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
        else:
            first = allOptions.pop(0)
            allOptions.append(first)
            menu(disp, allOptions)

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
        else:
            last = allOptions.pop()
            allOptions = [last] + allOptions
            menu(disp, allOptions)

    encoderLeftPrevoiusState = encoderLeftCurrentState
    encoderRightPrevoiusState = encoderRightCurrentState



def check_for_key_answer():
    if current_pub_key != "":
        management_ui()
    else:
        print("Room unavailable")
        # tutaj buzzer
        # choose_room()



def display_available_rooms():
    global current_pub_key
    current_pub_key = ""
    menu(disp, allOptions)


def choose_room(e):
    global current_room
    if current_pub_key == "":
        print(allOptions[0])
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
    GPIO.add_event_detect(buttonRed, GPIO.FALLING, callback=discard_room, bouncetime=20)
    GPIO.add_event_detect(buttonGreen, GPIO.FALLING, callback=choose_room, bouncetime=200)



def setup_broker():
    global disp
    disp = init_menu()

    bind_controllers()
    reset(disp)
    # display_available_rooms()
    client.loop_forever()


if __name__ == "__main__":
    unauthorized_ui()
    setup_broker()


