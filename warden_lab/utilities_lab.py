def is_room_number_valid(num):
    num = str(num)
    return num.isdigit() and 1 <= int(num) <= 100


