use rust_utils::*;

/*----------------------------------------------------------------------------*/
// Padding and Stripping

#[test]
fn test_padding() {
    let mut m1 = vec![
        100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115,
    ];
    let m1_pad: Vec<u8> = vec![
        100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 16, 16, 16,
        16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
    ];

    let mut m2 = vec![
        100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114,
    ];
    let m2_pad: Vec<u8> = vec![
        100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 1,
    ];

    let mut m3: Vec<u8> = vec![
        100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
    ];

    let m3_pad: Vec<u8> = vec![
        100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 2, 2,
    ];

    assert_eq!(pkcs7::pad(&mut m1), &m1_pad);
    assert_eq!(pkcs7::pad(&mut m2), &m2_pad);
    assert_eq!(pkcs7::pad(&mut m3), &m3_pad);
}