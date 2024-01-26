import paho.mqtt.client as mqtt
import threading
from encryption_utils_lab import decode_pub_key, encode_message
from utilities_lab import is_room_number_valid
from mfrc522 import MFRC522

from config import *

from buzzer import buzzer

from menu import init_menu, menu, display_brightness

broker_address = "test.mosquitto.org"

client = mqtt.Client()

authorized = False
can_read = True
answer = ""

current_room = 0

# pokoj jest liczba dodatnia, tak wiec -1 oznacza stan domyslny
last_room_pointer = -1

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


def unauthorized_ui():
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
        print(answer)
        # parse to rooms
        # deleguje zarzadzanie pokojami do petli while
        # display_available_rooms()
    else:
        buzzer(1)
        timeout_thread = threading.Timer(0.2, lambda: disable_buzzer())
        timeout_thread.start()
        authorized = False


def disable_buzzer():
    global can_read
    can_read = True
    buzzer(0)


def management_ui():
    print(f"Managing room {current_room}")


def handle_brightness_data(brightness):
    display_brightness(disp, brightness)


def discard_room(e):
    global current_pub_key
    global current_room
    # global last_room_pointer ?
    # znak zapytania, bo byc moze ten pointer zachowa sie dobrze, zobaczymy.
    if authorized:
        if current_pub_key != "":
            print(f"Exiting room {current_room}")
            current_room = 0
            # nie przechodze do zadnego ui - deleguje zarzadzanie do petli while
            current_pub_key = ""
            # last_room_pointer = allOptions[0]  ?
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
    if topic == keys_answers_topic and msg.payload.decode() != "":
        setup_current_public_key(msg)
    elif topic == brightness_status_topic:
        brightness = msg.payload.decode()
        handle_brightness_data(brightness)
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
    print(authorized)

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
        if authorized:
            last = allOptions.pop()
            allOptions = [last] + allOptions
            menu(disp, allOptions)

    encoderLeftPrevoiusState = encoderLeftCurrentState
    encoderRightPrevoiusState = encoderRightCurrentState


def check_for_key_answer():
    if current_pub_key != "":
        management_ui()
        # tutaj jest wyswietlanie ui przez event przeslania wiadomosci od pokoju
    else:
        print("Room unavailable")
        buzzer(1)
        timeout_thread = threading.Timer(0.4, disable_buzzer)
        timeout_thread.start()


def display_available_rooms():
    global current_pub_key
    global last_room_pointer
    current_pub_key = ""
    if last_room_pointer != allOptions[0]:
        menu(disp, allOptions)
        # blokada przed wielokrotnym rysowaniem na o-ledzie
        last_room_pointer = allOptions[0]


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
    client.connect(broker_address, keepalive=0, port=1883)

    subscribe()

    client.on_message = on_message_received

    bind_controllers()
    client.loop_start()



def parse_rooms_answer(answer):
    split = answer.split(',')
    temp = []
    for item in split:
        temp.append(int(item))
    print(temp)
    return temp


def setup_disp():
    global disp
    disp = init_menu()


def logout():
    global answer
    global can_read
    global authorized
    global allOptions
    global last_room_pointer
    answer = ""
    can_read = True
    authorized = False
    # nie ma co trzymac dostepnych pokoi poprzedniej osoby
    allOptions = []
    last_room_pointer = -1
    setup_disp()


if __name__ == "__main__":
    setup_disp()
    setup_broker()
    while True:
        if not authorized: # Jesli uzytkownik nie przylozyl karty lub nie ma dostepu
            unauthorized_ui()
        elif authorized and current_pub_key == "": # Jesli uzytkownik ma dostep do zarzadzania, ale nie wybral pokoju
            display_available_rooms()
        elif authorized and current_pub_key != "": # uzytkownik wybral pokok
            pass
            # Tutaj to jest chyba zarzadzane eventami.
            # Na kazda zmiane jasnosci pojawia sie klatka.
            # Patrz linia 124 - 126
            # Na każdą wiadomość od pokoju (jest to reakcja na rozkaz enkodera)
            # Jest pokazywana klatka na ekranie, nie powinno być tu problemu z rozmazaniem



