import paho.mqtt.client as mqtt
import keyboard
import threading
from encryption_utils import decode_pub_key, encode_message
from utilities import is_room_number_valid

# from config import *

# from buzzer import buzzer

broker_address = "test.mosquitto.org"

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


def send_command_to_server(command):
    # example "rooms:uid" -> List<Room>
    client.publish(server_command_topic, payload=str(command))


def unauthorized_ui():
    # on authorization -> send_command_to_server(f"rooms:{uid}")
    # wait for response, convert to list.
    # if list is empty -> buzzer! else authorized_ui(list)
    pass


def authorized_ui(rooms):
    pass


def management_ui():
    print(f"Managing room {current_room}")
    pass


def ask_for_rooms():
    pass


def handle_brightness_data(brightness):
    print(f"Brightness: {brightness}")
    pass


def discard_room(e):
    global current_pub_key
    global current_room
    if current_pub_key != "":
        print(f"Exiting room {current_room}")
        current_room = 0
        choose_room()


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


# def rotation_decode(d):
#     encb = GPIO.input(encoderRight)
#     if encb == 1:
#         message = str(0)
#     else:
#         message = str(1)
#     encoded_message = encode_message(message, current_pub_key)
#     client.publish(topic=encoder_topic, payload=encoded_message)


def publish_encoder_1(e):
    if current_pub_key != "":
        message = str(1)
        encoded_message = encode_message(message, current_pub_key)
        client.publish(topic=encoder_topic, payload=encoded_message)


def publish_encoder_0(e):
    if current_pub_key != "":
        message = str(0)
        encoded_message = encode_message(message, current_pub_key)
        client.publish(topic=encoder_topic, payload=encoded_message)


def check_for_key_answer():
    if current_pub_key != "":
        management_ui()
    else:
        print("Room unavailable")
        choose_room()


def choose_room():
    global current_room
    global current_pub_key
    current_pub_key = ""
    num = 0
    while not is_room_number_valid(num):
        num = input("Choose room number: ")
    current_room = int(num)
    client.publish(topic=keys_requests_topic, payload=str(current_room))
    timeout_thread = threading.Timer(0.25, check_for_key_answer)
    timeout_thread.start()


def bind_controllers():
    # GPIO.add_event_detect(encoderLeft, GPIO.FALLING, callback=rotation_decode, bouncetime=50)
    keyboard.on_press_key('e', publish_encoder_1)
    keyboard.on_press_key('q', publish_encoder_0)
    keyboard.on_press_key('x', discard_room)


def subscribe():
    client.subscribe(keys_answers_topic)
    client.subscribe(brightness_status_topic)
    client.subscribe(server_result_topic)


def setup_broker():
    client.connect(broker_address)

    subscribe()


    client.on_message = on_message_received

    choose_room()

    bind_controllers()

    client.loop_forever()


if __name__ == "__main__":
    while not authorized:
        unauthorized_ui()
    setup_broker()

