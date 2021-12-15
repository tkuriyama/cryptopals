from __future__ import division
import utils

def test_ECB_percentage(n):
    """Test detection of % of ECB from random ECB/CBC encryption."""
    f = utils.encrypt_ECB_CBC
    ECB_count = sum([utils.detect_ECB(f) for _ in xrange(n)])

    ECB_perc = ECB_count / n
    print 'Random ECB: % ECB in sample {:.0%}'.format(ECB_perc)
    assert ECB_perc > 0.3 and ECB_perc < 0.7
    return True

def test_always_ECB(n):
    """Check ECB detection oracle against ECB."""
    count = 0
    for _ in xrange(n):
        text = bytearray(['A'] * 100)
        code = utils.encrypt_ECB_CBC(text, True, True)
        blocks = utils.gen_blocks(code)
        count += 1 if utils.repeated_blocks(blocks, 3) else 0

    print 'Always ECB: % ECB in sample {:.0%}'.format(count / n)
    assert count == n
    return True

def test_always_CBC(n):
    """Check ECB detection oracle against CBC."""
    count = 0
    for _ in xrange(n):
        text = bytearray(['A'] * 100)
        key, iv = utils.gen_rand_key(), utils.gen_rand_key()
        code = utils.apply_CBC('encrypt', text, key, iv)
        blocks = utils.gen_blocks(code)
        count += 1 if utils.repeated_blocks(blocks, 3) else 0

    print 'Always CBC: % ECB in sample {:.0%}'.format(count / n)
    assert count == 0
    return True

def main():
    """Main."""
    n = 100

    test_ECB_percentage(n)
    test_always_ECB(n)
    test_always_CBC(n)

    print 'p11 ok'

if __name__ == '__main__':
    main()
