import utils

def main():
    """Main."""
    with open('p8_problem.txt', 'r') as f:
        data = f.read()
    codes = [bytearray.fromhex(line) for line in data.split('\n')]

    guesses = [utils.detect_AES_ECB(code) for code in codes]
    print [guess for guess in guesses if guess[0]]
    print 'p8 ok'

if __name__ == '__main__':
    main()
