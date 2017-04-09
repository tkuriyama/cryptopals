
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
