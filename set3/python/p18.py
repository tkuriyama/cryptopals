import base64
import struct
from typing import List
from Crypto.Cipher import AES

def main():
    code = base64.b64decode("L77na/nrFsKvynd6HzOoG7GHTLXsTVu9qvY/2syLXzhPweyyMTJULu/6/kXX0KSvoOLSFQ==")
    key = b"YELLOW SUBMARINE"
    nnce = 0
    stream = gen_keystream(key, 0, 0, len(code)//6)
    plaintext = bytearray([c1 ^ c2 for c1, c2 in zip(code, stream)])
    print(str(plaintext))
    return plaintext


def gen_keystream(key: List[bytes], nonce: int, ctr: int, n_blocks: int) -> List[bytes]:
    """Get CTR keystream."""
    AES_key = AES.new(key, AES.MODE_ECB)
    stream = []
    for i in range(n_blocks):
        ctr_msg = bytearray(struct.pack("<q", nonce) + struct.pack("<q", ctr + i))
        stream.extend(AES_key.encrypt(ctr_msg))
    return stream

if __name__ == '__main__':
    main()
