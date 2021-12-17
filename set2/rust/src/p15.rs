use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let valid = to_bytes::from_hex("4943452049434520424142590d0a04040404");
    let invalid1 = to_bytes::from_hex("4943452049434520424142590d0a05050505");
    let invalid2 = to_bytes::from_hex("4943452049434520424142590d0a0102030405");

    println!("Valid padding: {}", pkcs7::valid_padding(&valid));
    println!("Invalid padding: {}", pkcs7::valid_padding(&invalid1));
    println!("Invalid padding: {}", pkcs7::valid_padding(&invalid2));
}
