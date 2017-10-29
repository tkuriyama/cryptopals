import time
import hmac

from flask import Flask
from flask import request
app = Flask(__name__)


def apply_HMAC(text, key=b'YELLOW SUBMARINE'):
    """Apply SHA1-HMAC."""
    return hmac.HMAC(key, text, digestmod='sha1').hexdigest()

def secure_compare(input, guess):
    """Normal comparison."""
    return True if apply_HMAC(input) == guess else False

def pairs(s):
    """Split string into list of pairs."""
    return [s[i:i + 2] for i in range(0, len(s), 2)]

def insecure_compare(input, guess):
    """Insecure comparison."""
    hm = apply_HMAC(input)
    comp_pairs = zip(pairs(hm), pairs(guess))
    valid = True
    for fst, snd in comp_pairs:
        if fst != snd:
            valid = False
            break
        time.sleep(1/1000 * 50)
    return valid


@app.route("/test", methods=['GET'])
def test():    
   fname = request.args.get('file').encode()
   signature = request.args.get('signature')
   return (('200', 200) if secure_compare(fname, signature) else ('500', 500))

@app.route("/testinsecure", methods=['GET'])
def test_insecure():    
   fname = request.args.get('file').encode()
   signature = request.args.get('signature')
   return (('200', 200) if insecure_compare(fname, signature) else ('500', 500))

