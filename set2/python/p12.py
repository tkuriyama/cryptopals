import utils

def find_length(oracle):
    """Find block size of key using oracle.
    Feeding identical bytes into oracle, repeated output reveals block length.
    """
    block_len = 1
    text = bytearray('A' * block_len * 2)
    repeat = False
    max_len = 2048

    while not repeat and block_len < max_len:
        code = oracle(text)
        if code[: block_len] == code[block_len: block_len * 2]:
            repeat = True
        else:
            block_len += 1
            text = bytearray('A' * block_len * 2)
    return block_len

def main():
    """Main."""
    code_b64 = 'Um9sbGluJyBpbiBteSA1LjAKV2l0aCBteSByYWctdG9wIGRvd24gc28gbXkg'
    code_b64 += 'aGFpciBjYW4gYmxvdwpUaGUgZ2lybGllcyBvbiBzdGFuZGJ5IHdhdmluZyBq'
    code_b64 += 'dXN0IHRvIHNheSBoaQpEaWQgeW91IHN0b3A/IE5vLCBJIGp1c3QgZHJvdmUg'
    code_b64 += 'YnkK'
    code = bytearray(code_b64.decode('base64'))

    oracle = utils.gen_ECB_oracle(code)
    block_len = find_length(oracle)
    print '> Block size:', block_len
    print '> Oracle using ECB:', utils.detect_ECB(oracle)
    print '> Plaintext:\n', utils.decrypt_oracle_ECB(oracle, block_len, code)
    print '> p12 ok'

if __name__ == '__main__':
    main()
