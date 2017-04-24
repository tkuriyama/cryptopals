import utils

def main():
    """Main."""
    f = utils.strip_pkcs7

    str1 = bytearray('ICE ICE BABY\x04\x04\x04\x04')
    str2 = bytearray('ICE ICE BABY\x05\x05\x05\x05')
    str3 = bytearray('ICE ICE BABY\x01\x02\x03\x04')
    str4 = bytearray('ICE ICE BABY1234')
    str5 = bytearray('ICE ICE BABY1234' + '\x10' * 16)
    assert f(str1, False) == 'ICE ICE BABY'
    assert f(str2, False) is False
    assert f(str3, False) is False
    assert f(str4, False) is False
    assert f(str5, False) == 'ICE ICE BABY1234'

    for i in xrange(5, 20):
        rand = utils.gen_rand_key(i)
        block = utils.gen_blocks(rand)[-1]
        assert f(block, False) == block[:-(16 - i)]

    print 'p15 ok'

if __name__ == '__main__':
    main()
