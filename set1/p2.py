import utils

def main():
    """Main."""
    hex1 = '1c0111001f010100061a024b53535009181c'
    hex2 = '686974207468652062756c6c277320657965'
    hex3 = '746865206b696420646f6e277420706c6179'

    xor = utils.xor(bytearray.fromhex(hex1), bytearray.fromhex(hex2))
    assert ''.join([chr(c).encode('hex') for c in xor]) == hex3
    print 'p2 ok'

if __name__ == '__main__':
    main()
