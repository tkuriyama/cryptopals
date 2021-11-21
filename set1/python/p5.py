import utils

def main():
    """Main."""

    source = 'Burning \'em, if you ain\'t quick and nimble\n'
    source += 'I go crazy when I hear a cymbal'
    key = 'ICE'

    code = '0b3637272a2b2e63622c2e69692a23693a2a3c6324202d623d63343c2a26226324'
    code += '272765272a282b2f20430a652e2c652a3124333a653e2b2027630c692b2028316'
    code += '5286326302e27282f'

    xor_arr = utils.xor(bytearray(source), bytearray(key))
    assert ''.join([chr(c).encode('hex') for c in xor_arr]) == code
    print 'p5 ok'

if __name__ == '__main__':
    main()
