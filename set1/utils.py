"""Cryptonaalysis utilities for the cryptopals challenges."""

from __future__ import division

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

def xor(arr1, arr2):
    """XOR two byte arrays. Shorter array should be passed second."""
    if len(arr2) < len(arr1):
        l1, l2 = len(arr1), len(arr2)
        arr2 = arr2 * int(l1 / l2) + arr2[:l1 % l2]
    return [c1 ^ c2 for c1, c2 in zip(arr1, arr2)]

def chi_squared(text, ref_freq):
    """Return chi-squared test statistic of char frequency of text against ref.
    Penalize keys in text that are not in reference.
    Args
        text: sequence of chars, e.g. bytearray or str
        ref_freq: dict, frequency distribution of chars
    Returns
       Float of test statistic.
    """
    sample = [c.lower() for c in text]
    length = len(sample)
    sample_freq = dict([(c, sample.count(c) / length) for c in sample])

    scores = [(freq - ref_freq[c]) ** 2 / ref_freq[c] if c in ref_freq else 1
              for c, freq in sample_freq.iteritems()]
    return sum(scores)

def decrypt_single_xor(code, ref_freq={}):
    """Decrypt bytearray that has been encrypted with single-char XOR."""
    num_chars = len(code)
    keys = [(chr(i), bytearray(chr(i)) * num_chars) for i in xrange(256)]
    guesses = [(key, ''.join([chr(b) for b in xor(code, key_full)]))
               for key, key_full in keys]

    if not ref_freq:
        ref_freq = get_ref_freq_eng()
    scores = [(chi_squared(guess, ref_freq), key, guess)
              for key, guess in guesses]
    best_guess = min(scores, key=lambda x: x[0])
    return best_guess
