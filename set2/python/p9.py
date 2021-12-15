import utils

def main():
    """Main."""
    key = 'YELLOW SUBMARINE'
    padded = 'YELLOW SUBMARINE\x04\x04\x04\x04'
    assert utils.pad_pkcs7(20, key) == padded
    print 'p9 ok'

if __name__ == '__main__':
    main()
