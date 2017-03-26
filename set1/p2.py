
def hex_xor(str1, str2):
    """XOR two Hex strings."""
    arr1 = bytearray.fromhex(str1)
    arr2 = bytearray.fromhex(str2)
    xor_arr = [c1 ^ c2 for c1, c2 in zip(arr1, arr2)]
    xor_str = ''.join([chr(c).encode('hex') for c in xor_arr])
    return xor_str

def main():
    """Main."""
    hex1 = '1c0111001f010100061a024b53535009181c'
    hex2 = '686974207468652062756c6c277320657965'
    hex3 = '746865206b696420646f6e277420706c6179'

    assert hex_xor(hex1, hex2) == hex3
    print 'p2 ok'

if __name__ == '__main__':
    main()
