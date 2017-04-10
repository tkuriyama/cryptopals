from __future__ import division
from Crypto.Cipher import AES

# helpers

def xor(arr1, arr2):
    """XOR two byte arrays. Shorter array should be passed second."""
    if len(arr2) < len(arr1):
        l1, l2 = len(arr1), len(arr2)
        arr2 = arr2 * int(l1 / l2) + arr2[:l1 % l2]
    return bytearray(c1 ^ c2 for c1, c2 in zip(arr1, arr2))

# padding

def pad_pkcs7(key, block_len):
    """Pad given key using PKCS#7.
    Args
        key: byte array of key to pad
        block_len: length of block to which to pad key
    Returns
        bytearray of padded key
    """
    pad_len = max(0, block_len - len(key))
    return key + bytearray([pad_len] * pad_len)

def gen_blocks(code, pad, size=16):
    """Generate blocks from given bytearray code and pad function."""
    num_blocks = int(len(code) / size)
    blocks = [code[i * size: i * size + size] for i in xrange(num_blocks)]
    last = blocks[-1] if len(blocks[-1]) == size else pad(blocks[-1], size)
    return blocks[:-1] + [last]

# CBC

def CBC_encrypt(aes_f, blocks):
    """CBC encrypt: first XOR then encrypt."""
    prev = blocks[0]
    output = bytearray()
    for block in blocks[1:]:
        xor_arr = xor(prev, block)
        enc_arr = bytearray(aes_f(''.join([chr(b) for b in xor_arr])))
        prev = enc_arr
        output.extend(enc_arr)
    return output

def CBC_decrypt(aes_f, blocks):
    """CBC decyrpt: first decrypt then XOR."""
    prev = blocks[0]
    output = bytearray()
    for block in blocks[1:]:
        dec_arr = bytearray(aes_f(''.join([chr(b) for b in block])))
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
    key_AES = AES.new(key, AES.MODE_ECB)
    blocks = [iv] + gen_blocks(input, pad_pkcs7, len(key))
    return (CBC_encrypt(key_AES.encrypt, blocks) if mode == 'encrypt' else
            CBC_decrypt(key_AES.decrypt, blocks))
