import binascii

def hex_to_b64(hex_str):
    """Proper solution: Hex string -> byte array -> Base64 string."""
    bin = binascii.a2b_hex(hex_str)
    b64_str = binascii.b2a_base64(bin).strip()
    return b64_str

def main():
    """Main."""
    hex_str = '49276d206b696c6c696e6720796f757220627261696e206c696b65206120706'
    hex_str += 'f69736f6e6f7573206d757368726f6f6d'
    b64_str = 'SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hy'
    b64_str += 'b29t'

    assert hex_to_b64(hex_str) == b64_str
    print('p1 ok')

    return True

if __name__ == '__main__':
    main()
