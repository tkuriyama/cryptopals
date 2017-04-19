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

def gen_guesses(oracle, short):
    """Generate all possible full blocks given oracle and short block."""
    guesses = {}
    for n in xrange(256):
        code = oracle(short + bytearray([n]))[:16]
        guesses[str(code)] = bytearray([n])
    return guesses

def decrypt_block(oracle, block_len, block):
    """Brute-force decrypt ECB block using oracle."""
    guess = bytearray('A' * block_len)
    for ind in xrange(block_len):
        short = guess[1:]
        target = oracle(short, block[ind:])
        guesses = gen_guesses(oracle, short)
        found_byte = guesses[str(target[:16])]
        guess = guess[1:] + found_byte

    return guess

def decrypt_oracle_ECB(oracle, block_len, code):
    """Perform byte-at-a-time ECB decryption with given oracle.
    Args
        oracle: function, ECB oracle
        block_len: int, length of cipher block
        code: bytearray, ciphertext
    Returns
        bytearray of plaintext
    """
    plaintext = ''
    block, rest = code[:block_len], code[block_len:]
    
    while block or rest:
        decrypted = decrypt_block(oracle, block_len, block)
        plaintext += decrypted
        block, rest = rest[:block_len], rest[block_len:]

    return plaintext

def gen_oracle(full_code):
    """Return an ECB oracle function with fixed code to append and key."""
    key = utils.gen_rand_key()
    
    def call_encrypt(text, code=full_code):
        """Call ECB encryption with given code to append and fixed key."""
        return utils.encrypt_ECB_CBC(text, True, True, (bytearray(), code),
                                     key)
    return call_encrypt

def main():
    """Main."""
    code_b64 = 'Um9sbGluJyBpbiBteSA1LjAKV2l0aCBteSByYWctdG9wIGRvd24gc28gbXkg'
    code_b64 += 'aGFpciBjYW4gYmxvdwpUaGUgZ2lybGllcyBvbiBzdGFuZGJ5IHdhdmluZyBq'
    code_b64 += 'dXN0IHRvIHNheSBoaQpEaWQgeW91IHN0b3A/IE5vLCBJIGp1c3QgZHJvdmUg'
    code_b64 += 'YnkK'
    code = bytearray(code_b64.decode('base64'))
    
    oracle = gen_oracle(code)
    block_len = find_length(oracle)
    print '> Block size:', block_len
    print '> Oracle using ECB:', utils.detect_ECB(oracle)
    print '> Decrypted plaintext:\n', decrypt_oracle_ECB(oracle, block_len, code)
    print '> p12 ok'
    
if __name__ == '__main__':
    main()
