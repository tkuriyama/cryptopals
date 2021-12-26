use lazy_static::*;
use openssl::symm::Cipher;
use rust_utils::*;

/*----------------------------------------------------------------------------*/

lazy_static! {
    static ref KEY: Vec<u8> = random::rand_vec(16);
    static ref IV: Vec<u8> = random::rand_vec(16);
}

pub fn main() {
    let decrypter = decrypt_admin();

    let m1 = encrypt_cbc("XXXXX;admin=true");
    let output1 = aes::decrypt_cbc(Cipher::aes_128_ecb(), 16, &m1, &KEY, &IV).unwrap();
    println!(
        "Test roundtrip and quote special chars: admin = {}, {}",
        decrypter(&m1),
        from_bytes::to_utf8(&output1)
    );

    let m2 = encrypt_cbc("12345ZadminZtrue");
    let hacked = hack(&m2, &decrypter);
    let output2 = aes::decrypt_cbc(Cipher::aes_128_ecb(), 16, &hacked, &KEY, &IV).unwrap();
    println!(
        "Test bitflipped output: admin = {}\n{:?}, {:?}",
        decrypter(&hacked),
        from_bytes::to_utf8_by_block(&output2, 16, " -- garbled block -- "),
        output2
    );
}

/*----------------------------------------------------------------------------*/
// Oracles

fn encrypt_cbc(s: &str) -> Vec<u8> {
    let prepend = "comment1=cooking%20MCs;userdata=";
    let append = ";comment2=%20like%20a%20pound%20of%20bacon";
    let s_ = s.replace("=", "\"=\"").replace(";", "\";\"");
    let joined = String::from(prepend) + &s_ + append;
    let v = to_bytes::from_utf8(&joined);
    aes::encrypt_cbc(Cipher::aes_128_ecb(), 16, &v, &KEY, &IV)
}

fn decrypt_admin() -> impl Fn(&[u8]) -> bool {
    let decrypter = |v: &[u8]| -> bool {
        let v = aes::decrypt_cbc(Cipher::aes_128_ecb(), 16, v, &KEY, &IV).unwrap();
        let s = from_bytes::to_utf8_by_block(&v, 16, " -- garbled block -- ");
        s.split(";").into_iter().any(|elem| elem == "admin=true")
    };
    decrypter
}

/*----------------------------------------------------------------------------*/
// Attack

// 0123456789|12345
// 12345ZadminZtrue
fn hack(v: &[u8], decrypter: &impl Fn(&[u8]) -> bool) -> Vec<u8> {
    let mut hacked: Vec<u8> = Vec::new();
    'outer: for byte1 in 0..256 {
        for byte2 in 0..256 {
            hacked = flip(v, byte1 as u8, byte2 as u8);
            if decrypter(&hacked) == true {
                println!("Found XOR bytes: {:?}, {:?}", byte1, byte2);
                break 'outer;
            }
        }
    }
    hacked
}

fn flip(v: &[u8], b1: u8, b2: u8) -> Vec<u8> {
    let blocks = vector::to_blocks(v, 16);
    let block1 = blocks[1].clone();
    let block1_ = [
        block1[..5].to_vec(),
        vec![b1],
        block1[6..11].to_vec(),
        vec![b2],
        block1[12..].to_vec(),
    ]
    .concat();
    [blocks[0].clone(), block1_, blocks[2..].concat()].concat()
}
