use rust_utils::*;
mod p10;
mod p15;
mod p9;

/*----------------------------------------------------------------------------*/

fn main() {
    print::problem_separator("P9");
    p9::main();
    print::problem_separator("P10");
    p10::main();
    print::problem_separator("P15");
    p15::main();
}
