use lazy_static::*;
use rust_utils::*;

/*----------------------------------------------------------------------------*/

#[derive(Debug)]
struct Profile {
    email: String,
    id: u32,
    admin: bool,
}

/*----------------------------------------------------------------------------*/

lazy_static! {
    static ref KEY: Vec<u8> = random::rand_vec(16);
}

pub fn main() {
    let profile = parse_profile("email=foo@bar.com&uid=10&role=user");
    println!("Parse Not Admin: {:?}", profile);
    let profile = parse_profile("email=foo@bar.com&uid=10&role=admin ");
    println!("Parse Admin: {:?}", profile);

    println!("Test profile_for: {}", profile_for("hello@world.com"));
    println!(
        "Test profile_for -- strip delim chars: {}",
        profile_for("hello@world.com&role=admin")
    );

    let oracle = profile_oracle();
    println!(
        "Test encryot RT: {}",
        from_bytes::to_utf8(&decrypt_profile(&oracle("hello@world.com")))
    );

    let attack_profile = attack(&oracle);
    println!(
        "\nAttack profile (len {}): {:?}",
        attack_profile.len(),
        from_bytes::to_utf8(&decrypt_profile(&attack_profile))
    );
    println!(
        "Decrypted admin profile from oracle: {:?}",
        parse_profile(&from_bytes::to_utf8(&decrypt_profile(&attack_profile)))
    );
}

/*----------------------------------------------------------------------------*/
// Parsing

fn parse_profile(s: &str) -> Result<Profile, &'static str> {
    let s_: String = s.split_whitespace().collect();
    let pairs: Vec<&str> = s_.split("&").collect();

    if pairs.len() != 3 {
        Err("Expected three pairs of tokens")
    } else {
        let parsed_email = parse_email(pairs[0]);
        let is_admin = parse_role(pairs[2]);

        if parsed_email == None || is_admin == None {
            Err("Individual token parse failed")
        } else {
            let profile = Profile {
                email: parsed_email.unwrap(),
                id: random::rand_range(0, 100) as u32,
                admin: is_admin.unwrap(),
            };
            Ok(profile)
        }
    }
}

fn parse_email(s: &str) -> Option<String> {
    let v: Vec<&str> = s.split("=").collect();
    if v.len() != 2 {
        None
    } else {
        if v[0] == "email" {
            Some(v[1].to_string())
        } else {
            None
        }
    }
}

fn parse_role(s: &str) -> Option<bool> {
    let v: Vec<&str> = s.split("=").collect();
    if v.len() != 2 {
        None
    } else {
        if v[0] == "role" && v[1] == "admin" {
            Some(true)
        } else {
            Some(false)
        }
    }
}

/*----------------------------------------------------------------------------*/
// Oracle

fn profile_oracle() -> impl Fn(&str) -> Vec<u8> {
    let encrypter = move |email: &str| -> Vec<u8> {
        let msg = to_bytes::from_utf8(&profile_for(email));
        aes::encrypt_ecb(&msg, &KEY, None)
    };

    encrypter
}

fn decrypt_profile(v: &[u8]) -> Vec<u8> {
    aes::decrypt_ecb(v, &KEY, None)
}

fn profile_for(email: &str) -> String {
    let email_: String = email.chars().filter(|c| *c != '&' && *c != '=').collect();
    String::from("email=") + &email_ + "&id=placeholder&role=user"
}

/*----------------------------------------------------------------------------*/
// 0              |               |       8     14
// email=hello@world.com&id=placeholder&role=user

fn attack(oracle: &impl Fn(&str) -> Vec<u8>) -> Vec<u8> {
    let m1 = vector::to_blocks(&oracle("hello@worladmin           "), 16);
    let m2 = vector::to_blocks(&oracle("hello@world.com      "), 16);
    let m3 = vector::to_blocks(&oracle("hello@world.com01"), 16);

    [
        m2[0].to_vec(),
        m2[1].to_vec(),
        m2[2].to_vec(),
        m1[1].to_vec(),
        m3[3].to_vec(),
    ]
    .concat()
}
