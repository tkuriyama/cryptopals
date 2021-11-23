pub fn ascii_chars() -> Vec<u8> {
    let chars: Vec<u8> = (0..255).collect();
    chars
}

pub fn ascii_letters() -> Vec<u8> {
    let chars = ascii_chars()
        .iter()
        .filter(|&c| c.is_ascii_alphabetic())
        .map(|&c| c)
        .collect();
    chars
}
