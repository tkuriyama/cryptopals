use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let mut m1 = to_bytes::from_utf8("YELLOW SUBMARINE");
    let v1 = pkcs7::pad(&mut m1);

    let mut m2 = to_bytes::from_utf8("YELLOW SUBMARINE");
    let v2 = pkcs7::pad_n(&mut m2, 20);

    println!("{} {:?}", from_bytes::to_utf8(&v1), v1);
    println!("{} {:?}", from_bytes::to_utf8(&v2), v2);
}
