from __future__ import division
import json

def hex_xor(str1, str2):
    """XOR two Hex strings."""
    arr1 = bytearray.fromhex(str1)
    arr2 = bytearray.fromhex(str2)
    xor_arr = [c1 ^ c2 for c1, c2 in zip(arr1, arr2)]
    xor_str = ''.join([chr(c).encode('hex') for c in xor_arr])
    return xor_str

def freq_compare(guess, ref_freq):
    """Compare frequency of chars in guess vs reference (min score is best)."""
    text = guess.lower()
    chars = set(text)
    len_text = len(text)

    text_freq = dict([(char, text.count(char) / len_text) for char in chars])
    return sum([ref_freq[c] - text_freq[c] if c in text_freq else ref_freq[c]
                for c in ref_freq])

def decrypt_single_xor(code, ref_freq):
    """Decrypt code that has been encrypted with single-char XOR."""
    num_chars = int(len(code) / 2)
    keys = [chr(i).encode('hex') * num_chars for i in xrange(256)]

    guesses = [hex_xor(code, key).decode('hex') for key in keys]
    scores = [(guess, freq_compare(guess, ref_freq)) for guess in guesses]

    return min(scores, key=lambda x: x[1])

def main():
    """Main."""
    with open('p4_problem.txt', 'r') as f:
        data = f.read()
        lines = [line.strip() for line in data.split('\n') if line]

    with open('char_frequency.json', 'r') as f:
        ref_freq = json.load(f)

    line_scores = [decrypt_single_xor(line, ref_freq) for line in lines]
    print 'p4', min(line_scores, key=lambda x: x[1])

if __name__ == '__main__':
    main()
