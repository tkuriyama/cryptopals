use rust_utils::*;
use std::cmp;
use std::num::Wrapping;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let x = 0x9d2c5680 as u32;
    let y = mersenne::temper(x.clone());
    println!(
        "Original / Tempered / Untempered\n{} / {} / {}\n",
        x,
        y,
        untemper(y)
    );

    let mut mt = mersenne::seed(x);
    let mut mt_copy = mersenne::MTZERO;
    for i in 0..624 {
        mt_copy.state[i] = Wrapping(untemper(mt.next()));
    }
    mt_copy.index = 624;

    for _ in 0..10 {
        println!(
            "Original, / Repliacted:  {} / {}",
            mt.next(),
            mt_copy.next()
        );
    }
}

fn untemper(mut x: u32) -> u32 {
    x = undo_rshift(x, 18);
    x = undo_lshift(x, 15, 0xefc60000);
    x = undo_lshift(x, 7, 0x9d2c5680);
    x = undo_rshift(x, 11);
    x
}

/*----------------------------------------------------------------------------*/

fn undo_rshift(x: u32, n: u8) -> u32 {
    undo_rshift_helper(x, 0, 0, n - 1, n)
}

fn undo_rshift_helper(x: u32, acc: u32, start: u8, end: u8, size: u8) -> u32 {
    if end == 31 {
        return mask(x, start, end) ^ acc;
    }
    let block = mask(x, start, end) ^ acc;
    block | undo_rshift_helper(x, block >> size, end + 1, cmp::min(31, end + size), size)
}

fn undo_lshift(x: u32, n: u8, constant: u32) -> u32 {
    undo_lshift_helper(x, 0, 32 - n, 31, n, constant)
}

fn undo_lshift_helper(x: u32, acc: u32, start: u8, end: u8, size: u8, constant: u32) -> u32 {
    if start == 0 {
        return mask(x, start, end) ^ (acc & mask(constant, start, end));
    }
    let block = mask(x, start, end) ^ (acc & mask(constant, start, end));
    let new_start = if size > start { 0 } else { start - size };
    block | undo_lshift_helper(x, block << size, new_start, end - size, size, constant)
}

fn mask(x: u32, start: u8, end: u8) -> u32 {
    let left_masked = (x << start) >> start;
    (left_masked >> (31 - end)) << (31 - end)
}
