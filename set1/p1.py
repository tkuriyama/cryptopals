
def hex_to_b64(hex_str):
    """Convert Hex string to Base64 string."""
    arr = bytearray.fromhex(hex_str)
    decoded = ''.join([chr(c) for c in arr])
    b64_str = decoded.encode('base64').strip()
    return b64_str

def main():
    """Main."""
    hex_str = '49276d206b696c6c696e6720796f757220627261696e206c696b6520612070'
    hex_str += '6f69736f6e6f7573206d757368726f6f6d'
    b64_str = 'SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb2'
    b64_str += '9t'

    assert hex_to_b64(hex_str) == b64_str
    print('p1 ok')

if __name__ == '__main__':
    main()
