/*----------------------------------------------------------------------------*/
// Add PKCS7 padding, valid for u8 values

pub fn pad(v: &mut Vec<u8>) -> &mut Vec<u8> {
    pad_n(v, 16)
}

pub fn pad_n(v: &mut Vec<u8>, n: u8) -> &mut Vec<u8> {
    let byte = (n as usize) - v.len() % n as usize;
    let padding = [byte as u8].repeat(byte as usize);

    v.extend(padding);
    v
}

/*----------------------------------------------------------------------------*/
// Stripping and Validation

pub fn strip(v: &Vec<u8>) -> Option<Vec<u8>> {
    if valid_padding(v) {
        Some(strip_bytes(v, v[v.len() - 1]))
    } else {
        None
    }
}

fn strip_bytes(v: &Vec<u8>, n: u8) -> Vec<u8> {
    v[..(v.len() - (n as usize))].to_vec()
}

pub fn valid_padding(v: &Vec<u8>) -> bool {
    match v.len() {
        0 => false,
        n => verify_padding(v, v[n - 1] as usize),
    }
}

fn verify_padding(v: &Vec<u8>, pad_len: usize) -> bool {
    if pad_len > v.len() {
        false
    } else {
        all_equal(&v[(v.len() - pad_len)..])
    }
}

fn all_equal(v: &[u8]) -> bool {
    v.iter().min() == v.iter().max()
}
