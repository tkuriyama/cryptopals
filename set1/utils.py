"""Cryptonaalysis utilities for the cryptopals challenges."""

from __future__ import division
from collections import defaultdict
from Crypto.Cipher import AES

# Helpers

def get_blocks(code, size, n=4):
    """Get first n blocks of given size (int) from code."""
    assert size * n <= len(code)
    return [code[i * size: (i + 1) * size] for i in xrange(n)]

# Edit Distance

def arr_to_bin(arr):
    """Byte array -> string of binary representation (8 bits per byte)."""
    return ''.join([bin(b)[2:].zfill(8) for b in arr])

def edit_dist(arr1, arr2):
    """Find edit distance between two byte arrays (as number of dif bits)."""
    assert len(arr1) == len(arr2)
    xor_arr = [0 if b1 == b2 else 1
               for b1, b2 in zip(arr_to_bin(arr1), arr_to_bin(arr2))]
    return sum(xor_arr)

def test_edit_dist():
    """Test case for edit_dist()."""
    str1 = 'this is a test'
    str2 = 'wokka wokka!!!'
    assert edit_dist(bytearray(str1), bytearray(str2)) == 37

# Scoring

def get_ref_freq_eng():
    """Dict of frequency distribution of English chars incl. space."""
    d = {'a': 0.0651738, 'b': 0.0124248, 'c': 0.0217339,
         'd': 0.0349835, 'e': 0.1041442, 'f': 0.0197881,
         'g': 0.0158610, 'h': 0.0492888, 'i': 0.0558094,
         'j': 0.0009033, 'k': 0.0050529, 'l': 0.0331490,
         'm': 0.0202124, 'n': 0.0564513, 'o': 0.0596302,
         'p': 0.0137645, 'q': 0.0008606, 'r': 0.0497563,
         's': 0.0515760, 't': 0.0729357, 'u': 0.0225134,
         'v': 0.0082903, 'w': 0.0171272, 'x': 0.0013692,
         'y': 0.0145984, 'z': 0.0007836, ' ': 0.1918182}
    return d

def chi_squared_calc(c, freq, ref_freq):
    """Return chi-squared calc for individual observation.
    Ignore common puctuation and penalize unknown characters."""
    return ((freq - ref_freq[c]) ** 2 / ref_freq[c] if c in ref_freq else
            0 if c in ('\n', '\'', '"', ',', '-', '.', '?', '!', '-') else
            1)

def chi_squared(text, ref_freq):
    """Return chi-squared test statistic of char frequency of text against ref.
    Args
        text: sequence of chars, e.g. bytearray or str
        ref_freq: dict, frequency distribution of chars
    Returns
       Float of test statistic.
    """
    sample = [c.lower() for c in text]
    length = len(sample)
    sample_freq = dict([(c, sample.count(c) / length) for c in sample])

    scores = [chi_squared_calc(c, freq, ref_freq)
              for c, freq in sample_freq.iteritems()]
    return sum(scores)

def score_guesses(guesses, ref_freq={}):
    """Score list of guesses [key, guess] with chi-squared stat."""
    if not ref_freq:
        ref_freq = get_ref_freq_eng()
    return [(chi_squared(guess, ref_freq), key, guess)
            for key, guess in guesses]

# XOR

def xor(arr1, arr2):
    """XOR two byte arrays. Shorter array should be passed second."""
    if len(arr2) < len(arr1):
        l1, l2 = len(arr1), len(arr2)
        arr2 = arr2 * int(l1 / l2) + arr2[:l1 % l2]
    return [c1 ^ c2 for c1, c2 in zip(arr1, arr2)]

def decrypt_single_xor(code, ref_freq={}):
    """Decrypt bytearray that has been encrypted with single-char XOR.
    Brute force all 256 possible keys and return best guess.
    Args
        code: bytearray of code to decrypt
        ref_freq: dict of reference char frequency, optional
    Returns
        (score, key, plaintext guess)
    """
    num_chars = len(code)
    keys = [(chr(i), bytearray(chr(i)) * num_chars) for i in xrange(256)]
    guesses = [(key, ''.join([chr(b) for b in xor(code, key_full)]))
               for key, key_full in keys]

    scores = score_guesses(guesses)
    return min(scores, key=lambda x: x[0])

# AES

def detect_AES_ECB(code):
    """Detect if code is encrypted in AES ECB mode.
    Returns
        (flag if ECB mode likely, dict of any blocks occuring more than once)
    """
    blocks = get_blocks(code, 16, int(len(code) / 16))

    freq_dict = defaultdict(int)
    for block in blocks:
        block_hex = ''.join([chr(b).encode('hex') for b in block])
        freq_dict[block_hex] += 1

    high_freq = [(block, freq) for block, freq in freq_dict.iteritems()
                 if freq > 1]
    return True if high_freq else False, high_freq
