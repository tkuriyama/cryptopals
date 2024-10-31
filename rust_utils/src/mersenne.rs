// Based on https://en.wikipedia.org/wiki/Mersenne_Twister
//use crate::*;
use crate::*;
use std::num::Wrapping;

/*----------------------------------------------------------------------------*/

const N: usize = 624;
const M: usize = 397;
const ZERO: Wrapping<u32> = Wrapping(0);
const A: Wrapping<u32> = Wrapping(0x9908b0df);
const UPPER_MASK: Wrapping<u32> = Wrapping(0x80000000);
const LOWER_MASK: Wrapping<u32> = Wrapping(0x7fffffff);

pub struct MT {
    pub index: usize,
    pub state: [Wrapping<u32>; N],
}

pub const MTZERO: MT = MT {
    index: 1,
    state: [Wrapping(0); N],
};

/*----------------------------------------------------------------------------*/

pub fn seed(seed: u32) -> MT {
    let mut mt = MTZERO;
    mt.index = N;
    mt.state[0] = Wrapping(seed);
    for i in 1..N {
        mt.state[i] =
            Wrapping(1812433253) * (mt.state[i - 1] ^ (mt.state[i - 1] >> 30)) + Wrapping(i as u32);
    }
    mt
}

impl MT {
    pub fn next(&mut self) -> u32 {
        if self.index >= N {
            self.twist();
        }
        let Wrapping(x) = self.state[self.index];
        self.index += 1;
        temper(x)
    }

    fn twist(&mut self) {
        for i in 0..(N - 1) {
            let x = (self.state[i] & UPPER_MASK)
                | ((self.state[i + 1] % Wrapping(N as u32)) & LOWER_MASK);
            let mut xa = x >> 1;
            if (x % Wrapping(2)) != ZERO {
                xa = xa ^ A;
            }
            self.state[i] = self.state[(i + M) % N] ^ xa;
        }
        self.index = 0;
    }
}

pub fn temper(mut x: u32) -> u32 {
    x ^= x >> 11;
    x ^= (x << 7) & 0x9d2c5680;
    x ^= (x << 15) & 0xefc60000;
    x ^= x >> 18;
    x
}

/*----------------------------------------------------------------------------*/

pub fn apply_ctr(msg: &[u8], key: u16) -> Vec<u8> {
    let mut mt = seed(key as u32);
    let keystream: Vec<u8> = std::iter::repeat(())
        .take(msg.len())
        .map(|_| mt.next() as u8)
        .collect();
    vector::xor(msg, &keystream)
}
