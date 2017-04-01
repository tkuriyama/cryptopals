from __future__ import division

def expand_key(key, length):
    """Expand key to string of given length by repetition."""
    return key * int(length / len(key)) + key[:length % len(key)]

def encode_xor(text, key):
    """Encrypt plaintext w/ key using repeating XOR; return as hex."""
    full_key = expand_key(key, len(text))
    xor_arr = [c1 ^ c2 for c1, c2 in zip(bytearray(text), bytearray(full_key))]
    xor_str = ''.join([chr(c).encode('hex') for c in xor_arr])
    return xor_str

def main():
    """Main."""

    source = 'Burning \'em, if you ain\'t quick and nimble\n'
    source += 'I go crazy when I hear a cymbal'
    key = 'ICE'

    code = '0b3637272a2b2e63622c2e69692a23693a2a3c6324202d623d63343c2a26226324'
    code += '272765272a282b2f20430a652e2c652a3124333a653e2b2027630c692b2028316'
    code += '5286326302e27282f'

    print encode_xor(source, key)
    assert encode_xor(source, key) == code
    print 'p5 ok'

if __name__ == '__main__':
    main()
