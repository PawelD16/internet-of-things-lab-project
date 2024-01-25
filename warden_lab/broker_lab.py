import paho.mqtt.client as mqtt
import keyboard
import threading
from encryption_utils import decode_pub_key, encode_message
from utilities import is_room_number_valid

from config import *

from buzzer import buzzer

from menu import init_menu, menu, display_brightness

broker_address = "10.108.33.122"

client = mqtt.Client()

authorized = True

current_room = 0

encoder_topic = "messages/encoder"
keys_requests_topic = "messages/key/requests"
keys_answers_topic = "messages/key/answers"
brightness_status_topic = "messages/brightness"

server_command_topic = "server/command"
server_result_topic = "server/result"

current_pub_key = ""

allOptions = [1, 4, 7, 9]

encoderLeftPrevoiusState = GPIO.input(encoderLeft)
encoderRightPrevoiusState = GPIO.input(encoderRight)

disp = None



def send_command_to_server(command):
    # example "rooms:uid" -> List<Room>
    client.publish(server_command_topic, payload=str(command))


def unauthorized_ui():
    while not authorized:
    # dopoki unauthorized -> czytaj karte
    # on authorization -> send_command_to_server(f"rooms:{uid}")
    # wait for response, convert to list.
    # if list is empty -> buzzer! else authorized_ui(list)
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


def on_message_received(c, userdata, msg):
    topic = msg.topic
    if topic == keys_answers_topic and msg.payload.decode() != "":
        setup_current_public_key(msg)
    elif topic == brightness_status_topic:
        brightness = msg.payload.decode()
        handle_brightness_data(brightness)
    elif topic == server_result_topic:
        print(f"Response from server: {msg.payload.decode()}")


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


def subscribe():
    client.subscribe(keys_answers_topic)
    client.subscribe(brightness_status_topic)
    client.subscribe(server_result_topic)


def setup_broker():
    global disp
    disp = init_menu()
    client.connect(broker_address, keepalive=0)

    subscribe()

    client.on_message = on_message_received

    bind_controllers()

    display_available_rooms()

    client.loop_forever()


if __name__ == "__main__":
    unauthorized_ui()
    setup_broker()


