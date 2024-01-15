import rsa
import os


def check_if_keys_exist():
    if not os.path.exists("./keys"):
        os.mkdir("./keys")
        return False
    pub_exists = os.path.exists(os.path.join("./keys", "public.pem"))
    priv_exists = os.path.exists(os.path.join("./keys", "private.pem"))
    return pub_exists and priv_exists


def create_key_pair(bytes):
    if not check_if_keys_exist():
        pub_key, priv_key = rsa.newkeys(bytes)

        pub_path = os.path.join("./keys", "public.pem")
        priv_path = os.path.join("./keys", "private.pem")

        with open(pub_path, "wb") as f:
            f.write(pub_key.save_pkcs1("PEM"))

        with open(priv_path, "wb") as f:
            f.write(priv_key.save_pkcs1("PEM"))
        print(f"Keys generated successfully in")
    else:
        print("Key files already exist, aborting creation.")


def decode_pub_key(key_str):
    key_data = key_str.replace('PublicKey(', '').replace(')', '').split(', ')
    modulus, exponent = int(key_data[0]), int(key_data[1])
    key = rsa.PublicKey(modulus, exponent)
    return key


def get_public_key():
    if not check_if_keys_exist():
        create_key_pair(1024)
    with open("./keys/public.pem", "rb") as f:
        pub_key = rsa.PublicKey.load_pkcs1(f.read())
        return pub_key


def decode_message(message):
    if not check_if_keys_exist():
        return None
    with open("./keys/private.pem", "rb") as f:
        priv_key = rsa.PrivateKey.load_pkcs1(f.read())
        decoded_message = rsa.decrypt(message, priv_key)
        return decoded_message


def encode_message(message, key):
    encoded_message = rsa.encrypt(message.encode(), key)
    return encoded_message

