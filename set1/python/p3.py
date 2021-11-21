import utils

def main():
    """Main."""
    code = '1b37373331363f78151b7f2b783431333d78397828372d363c78373e783a393b37'
    code += '36'

    guess = utils.decrypt_single_xor(bytearray.fromhex(code))
    print 'p3', guess

if __name__ == '__main__':
    main()
