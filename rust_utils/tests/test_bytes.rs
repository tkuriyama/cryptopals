use rust_utils::*;

/*----------------------------------------------------------------------------*/
// Encode-Decode Roundtrips

#[test]
fn test_hex_roundtrip() {
    let s = "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";
    assert_eq!(s, from_bytes::to_hex(&to_bytes::from_hex(&s)));
}

#[test]
fn test_b64_roundtrip() {
    let s = "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";
    assert_eq!(s, from_bytes::to_b64(&to_bytes::from_b64(&s)));
}
#[test]
fn test_utf8_roundtrip() {
    let s = "Hello, World!";
    assert_eq!(s, from_bytes::to_utf8(&to_bytes::from_utf8(&s)));
}
