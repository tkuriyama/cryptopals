pub fn pad(v: &mut Vec<u8>) -> &mut Vec<u8> {
    pad_n(v, 16)
}

pub fn pad_n(v: &mut Vec<u8>, n: u8) -> &mut Vec<u8> {
    let byte = (n as usize) - v.len() % n as usize;
    let padding = [byte as u8].repeat(byte as usize);

    v.extend(padding);
    v
}
