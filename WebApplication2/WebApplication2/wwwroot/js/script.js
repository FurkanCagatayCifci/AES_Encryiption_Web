// script.js

let encryptionKey;  // Şifreleme için anahtar
let encryptedData;  // Şifrelenmiş veri

// Rastgele anahtar oluşturma
async function generateKey() {
    encryptionKey = await window.crypto.subtle.generateKey(
        {
            name: "AES-GCM",
            length: 256
        },
        true,
        ["encrypt", "decrypt"]
    );
    document.getElementById('keyDisplay').textContent = "Anahtar oluşturuldu!";
}

// Veriyi şifreleme
async function encryptData() {
    const data = document.getElementById("inputData").value;
    if (!data || !encryptionKey) {
        alert("Lütfen veri girin ve anahtar oluşturun!");
        return;
    }

    const encoder = new TextEncoder();
    const dataBuffer = encoder.encode(data);
    const iv = window.crypto.getRandomValues(new Uint8Array(12)); // Initialization Vector

    try {
        encryptedData = await window.crypto.subtle.encrypt(
            {
                name: "AES-GCM",
                iv: iv
            },
            encryptionKey,
            dataBuffer
        );

        document.getElementById("encryptedDisplay").textContent = `Şifrelenmiş Veri: ${bufferToHex(encryptedData)}`;
        alert("Şifreleme başarılı!");
    } catch (error) {
        console.error("Şifreleme hatası:", error);
    }
}

// Şifreyi çözme
async function decryptData() {
    if (!encryptedData || !encryptionKey) {
        alert("Şifrelenmiş veri veya anahtar bulunamadı!");
        return;
    }

    const iv = new Uint8Array(12); // Şifreleme sırasında kullandığımız iv'yi tekrar aynı şekilde belirtmeliyiz
    const decoder = new TextDecoder();

    try {
        const decryptedBuffer = await window.crypto.subtle.decrypt(
            {
                name: "AES-GCM",
                iv: iv
            },
            encryptionKey,
            encryptedData
        );

        const originalData = decoder.decode(decryptedBuffer);
        document.getElementById("decryptedDisplay").textContent = `Orijinal Veri: ${originalData}`;
    } catch (error) {
        console.error("Şifre çözme hatası:", error);
    }
}

// Buffer'ı hex stringe çevirme (Şifrelenmiş veriyi ekranda göstermek için)
function bufferToHex(buffer) {
    const byteArray = new Uint8Array(buffer);
    return Array.from(byteArray, byte => byte.toString(16).padStart(2, '0')).join('');
}
