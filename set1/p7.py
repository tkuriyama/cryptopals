from Crypto.Cipher import AES
import utils

def main():
    """Main."""

    with open('p7_problem.txt', 'r') as f:
        data = f.read()
    code = data.decode('base64')

    key = 'YELLOW SUBMARINE'
    decryptor = AES.new(key, AES.MODE_ECB)
    print decryptor.decrypt(code)
    print 'p7 ok'

if __name__ == '__main__':
    main()
