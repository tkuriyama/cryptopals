from __future__ import division
import random
from Crypto.Cipher import AES

# helpers

def xor(arr1, arr2):
    """XOR two byte arrays. Shorter array should be passed second."""
    if len(arr2) < len(arr1):
        l1, l2 = len(arr1), len(arr2)
        arr2 = arr2 * int(l1 / l2) + arr2[:l1 % l2]
    return bytearray(c1 ^ c2 for c1, c2 in zip(arr1, arr2))

def gen_rand_key(size=16):
    """Generate random key (default 16 bytes)."""
    return bytearray([random.SystemRandom().randint(0, 255)
                      for _ in xrange(size)])

# padding

def pad_pkcs7(key, block_len=16):
    """Pad given key using PKCS#7.
    Args
        key: byte array of key to pad
        block_len: length of block to which to pad key
    Returns
        bytearray of padded key
    """
    pad_len = max(0, block_len - len(key))
    return key + bytearray([pad_len] * pad_len)

def gen_blocks(code, pad=pad_pkcs7, size=16):
    """Generate blocks from given bytearray code and pad function."""
    num_blocks = int(len(code) / size)
    blocks = [code[i * size: i * size + size] for i in xrange(num_blocks)]
    last = blocks[-1] if len(blocks[-1]) == size else pad(blocks[-1], size)
    return blocks[:-1] + [last]

# ECB

def apply_ECB(mode, input, key, pad=pad_pkcs7):
    """Apply ECB to given key and inputs.
    Args
        mode: str, 'encrypt' or 'decrypt'
        key: str, key to use as AES key
        input: bytearray of input code
        pad: padding function to use on input
    Returns
        Bytearray of encrypted or decrypted input.
    """
    blocks = gen_blocks(input, pad)
    input_str = ''.join([str(block) for block in blocks])
    aes_f = key.encrypt if mode == 'encrypt' else key.decrypt
    return bytearray(aes_f(input_str))

# CBC

def CBC_encrypt(aes_f, blocks):
    """CBC encrypt: first XOR then encrypt."""
    prev = blocks[0]
    output = bytearray()
    for block in blocks[1:]:
        xor_arr = xor(prev, block)
        enc_arr = bytearray(aes_f(str(xor_arr)))
        prev = enc_arr
        output.extend(enc_arr)
    return output

def CBC_decrypt(aes_f, blocks):
    """CBC decyrpt: first decrypt then XOR."""
    prev = blocks[0]
    output = bytearray()
    for block in blocks[1:]:
        dec_arr = bytearray(aes_f(str(block)))
        xor_arr = xor(dec_arr, prev)
        prev = block[:]
        output.extend(xor_arr)
    return output

def apply_CBC(mode, input, key, iv, block_len=16):
    """Apply CBC to given input, key, and iv values.
    Args
        mode: str, 'encrypt' or 'decrypt'
        input: bytearray of intput code
        key: str, key to use as AES key
        iv: bytearray, initialization vector for CBC
        block_len: int, optional, normally 16 for block size
    Returns
        Bytearray of encrypted or decrypted input.
    """
    key_AES = AES.new(str(key), AES.MODE_ECB)
    blocks = [iv] + gen_blocks(input, pad_pkcs7, len(key))
    return (CBC_encrypt(key_AES.encrypt, blocks) if mode == 'encrypt' else
            CBC_decrypt(key_AES.decrypt, blocks))

# ECB / CBC Encryption and Detection Oracle

def encrypt_ECB_CBC(text, noise=True, force_ECB=False):
    """Encrypt given text in ECB or CBC, randomly with noice unless specified.
    Args
        text: bytearray of plaintext to encrypt
        noise: bool, add 5 - 10 random before and after text, default True
        force_ECB: bool, force ECB if set to True, default False
    Returns
        Bytearray of encrypted text.
    """
    if noise:
        size1 = random.SystemRandom().randint(5, 10)
        size2 = random.SystemRandom().randint(5, 10)
        text = gen_rand_key(size1) + text + gen_rand_key(size2)

    key = gen_rand_key()
    ECB = random.SystemRandom().randint(0, 1)

    if ECB or force_ECB:
        code = apply_ECB('encrypt', text, AES.new(str(key), AES.MODE_ECB))
    else:
        code = apply_CBC('encrypt', text, key, gen_rand_key())
    return code

def repeated_blocks(blocks, threshold=2):
    """Retrun True if list of blocks > count of dups more than theshold."""
    blocks_list = [str(block) for block in blocks]
    blocks_set = set(blocks_list)
    return len(blocks_list) - len(blocks_set) > threshold

def detect_ECB(encrypt):
    """Return True if given function encrypts with ECB, else False.
    Check for repeated blocks given deterministic input.
    """
    text = bytearray(['A'] * 100)
    code = encrypt(text)
    blocks = gen_blocks(code)
    return True if repeated_blocks(blocks, 3) else False

# tests

def test_ECB_symmetry():
    """Test ECB encrypt -> decrypt yields original."""
    text = 'test' * 16
    key = gen_rand_key()
    AES_key = AES.new(str(key), AES.MODE_ECB)

    cipher = apply_ECB('encrypt', text, AES_key)
    plain = apply_ECB('decrypt', cipher, AES_key)
    assert text == plain

    return True

def test_CBC_symmetry():
    """Test CBC encrypt -> decrypt yields original"""
    text = bytearray('test' * 16)
    key = gen_rand_key()
    iv = gen_rand_key()

    cipher = apply_CBC('encrypt', text, key, iv)
    plain = apply_CBC('decrypt', cipher, key, iv)
    assert text == plain

    return True

