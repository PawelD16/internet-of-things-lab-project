import paho.mqtt.client as mqtt
from utilities import is_room_number_valid
from encryption_utils import get_public_key, decode_message


pixels = None

broker_address = "test.mosquitto.org"

room_number = 0
client = mqtt.Client()
brightness = 0

encoder_topic = "messages/encoder"
keys_requests_topic = "messages/key/requests"
keys_answers_topic = "messages/key/answers"
brightness_status_topic = "messages/brightness"


def setup_diodes():
    # pixels = neopixel.NeoPixel(board.D18, 8, brightness=brightness / 32, auto_write=False)
    pass


def update_diodes():
    # pixels.brightness = brightness / 32
    # pixels.show()
    pass


def parse_encoder_data(msg):
    global brightness
    msg = int(msg)
    if msg == 1:
        if brightness < 32:
            brightness += 1
    elif msg == 0:
        if brightness > 0:
            brightness -= 1
    update_diodes()
    client.publish(topic=brightness_status_topic, payload=str(brightness))


def message_router(c, userdata, msg):
    topic = msg.topic
    if topic == encoder_topic:
        mess = msg.payload
        # decode fail -> exception
        try:
            decoded = decode_message(mess)
            parse_encoder_data(decoded)
        except Exception:
            print("decrypt fail")
    elif topic == keys_requests_topic:
        num = msg.payload.decode()
        if int(num) == int(room_number):
            key = get_public_key()
            client.publish(topic=keys_answers_topic, payload=str(key))
            client.publish(topic=brightness_status_topic, payload=str(brightness))


def subscribe():
    client.subscribe(encoder_topic)
    client.subscribe(keys_requests_topic)


def connect_to_broker():
    client.connect(broker_address)
    print(f"Connected to broker@{broker_address}")
    subscribe()

    client.on_message = message_router

    client.loop_forever()


def initialize_room():
    global room_number
    num = 0
    while not is_room_number_valid(num):
        num = input("Choose room number (1 - 100): ")
    room_number = num


if __name__ == "__main__":
    initialize_room()
    setup_diodes()
    try:
        connect_to_broker()
    except TimeoutError:
        print(f"Can't connect to broker@{broker_address} !")

