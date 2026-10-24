using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.Security.Cryptography;

namespace AutoParts.BusinessLogic
{
    public static class PasswordHelper
    {
        private const int Iteraciones = 600000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string CrearHash(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException(
                    "La contraseña no puede estar vacía.");

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                contrasena,
                salt,
                Iteraciones,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"PBKDF2-SHA256${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verificar(
            string contrasena,
            string hashGuardado)
        {
            if (string.IsNullOrEmpty(contrasena) ||
                string.IsNullOrEmpty(hashGuardado))
                return false;

            string[] partes = hashGuardado.Split('$');

            if (partes.Length != 4 ||
                partes[0] != "PBKDF2-SHA256" ||
                !int.TryParse(partes[1], out int iteraciones) ||
                iteraciones < 100000 ||
                iteraciones > 2000000)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(partes[2]);
                byte[] hashOriginal = Convert.FromBase64String(partes[3]);

                if (salt.Length != SaltSize ||
                    hashOriginal.Length != HashSize)
                    return false;

                byte[] hashNuevo = Rfc2898DeriveBytes.Pbkdf2(
                    contrasena,
                    salt,
                    iteraciones,
                    HashAlgorithmName.SHA256,
                    HashSize);

                return CryptographicOperations.FixedTimeEquals(
                    hashNuevo,
                    hashOriginal);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
