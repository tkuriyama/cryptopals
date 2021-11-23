pub fn f32_eq(f1: f32, f2: f32, epsilon: f32) -> bool {
    let diff = (f1 - f2).abs();
    diff < epsilon.abs()
}
