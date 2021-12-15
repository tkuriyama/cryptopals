import utils

def main():
    """Main."""
    code_b64 = 'Um9sbGluJyBpbiBteSA1LjAKV2l0aCBteSByYWctdG9wIGRvd24gc28gbXkg'
    code_b64 += 'aGFpciBjYW4gYmxvdwpUaGUgZ2lybGllcyBvbiBzdGFuZGJ5IHdhdmluZyBq'
    code_b64 += 'dXN0IHRvIHNheSBoaQpEaWQgeW91IHN0b3A/IE5vLCBJIGp1c3QgZHJvdmUg'
    code_b64 += 'YnkK'
    code = bytearray(code_b64.decode('base64'))

    oracle = utils.gen_ECB_oracle(code, 20)
    print '> Oracle using ECB:', utils.detect_ECB(oracle)
    for i in xrange(1, 11):
        try:
            decrypted = utils.decrypt_oracle_ECB(oracle, 16, code, 20)
            break
        except KeyError:
            print 'try {:d} failed, trying again'.format(i)
    if decrypted:
        print '> Plaintext:\n', decrypted
        print '> p14 ok'
    else:
        print '> p14 failed -- unable to decipher after several tries'

if __name__ == '__main__':
    main()
