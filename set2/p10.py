import utils

def main():
    """Main."""
    with open('p10_problem.txt', 'r') as f:
        data = f.read()
        code = bytearray(data.decode('base64'))

    key = 'YELLOW SUBMARINE'
    iv = bytearray([0] * len(key))

    decrypted = utils.apply_CBC('decrypt', code, key, iv)
    print ''.join([chr(b) for b in decrypted])
    print 'p10 ok'

if __name__ == '__main__':
    main()
