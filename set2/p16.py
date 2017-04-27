import utils

def encrypt_CBC(text, key):
    """Encrypt CBC per prompt."""
    pre = bytearray('comment1=cooking%20MCs;userdata=')
    post = bytearray(';comment2=%20like%20a%20pound%20of%20bacon')

    safe_text = text
    for char in (';='):
        safe_text = safe_text.replace(char, '"{}"'.format(char))
    
    input = pre + safe_text + post
    iv = bytearray([0] * 16)
    return utils.apply_CBC('encrypt', input, key)
    
def decrypt_CBC(code, key):
    """Decrypt CBC."""
    return utils.apply_CBC('decrypt', code, key)

def decrypt_and_find(code, key):
    """Decrypt CBC and look for string ';admin=True;'."""
    decrypted = decrypt_CBC(code, key)
    target = ';admin=True;'
    return target in decrypted

def find_mod(code, key, target, attack_ind, target_ind):
    """Find modification to attack_ind that yields target in target_ind."""
    match = ''
    for mod in xrange(1, 256):
        code[attack_ind] = mod
        decrypted = decrypt_CBC(code, key)
        if chr(decrypted[target_ind]) == target:
            match = mod
            break
    return match

def hack_CBC(key):
    """Hack CBC encrypted code """
    text = 'ZadminZTrueZ'
    code = encrypt_CBC(text, key)

    targets = ((0, bytearray(';')),
               (6, bytearray('=')),
               (11, bytearray(';')))

    attack_start = 16
    target_start = 32

    for ind, target in targets:
        attack_ind = attack_start + ind
        target_ind = target_start + ind
        mod = find_mod(code, key, target, attack_ind, target_ind)
        code[attack_ind] = mod
    
    return code

def main():
    """Main."""
    key = utils.gen_rand_key()
    text = bytearray('testinput')
    
    code = encrypt_CBC(text, key)
    print '> encrypted\n', code
    print '\n> decrypted\n', decrypt_CBC(code, key)
    print '\n> target in decrypted\n', decrypt_and_find(code, key)
    hacked = hack_CBC(key)
    # print '\n> hacked code\n', hacked
    print '\n> hacked code decrypted\n', decrypt_CBC(hacked, key)
    print '\n> target in hacked\n', decrypt_and_find(hacked, key)
    
    print '\np16 ok'

if __name__ == '__main__':
    main()
