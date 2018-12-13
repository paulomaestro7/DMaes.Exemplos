namespace DMaes.Exemplos.Autenticacao.Jwt.Identity.Util
{
    public static class ApplicationMode
    {
        public static Mode Mode { get; set; }
    }

    public enum Mode
    {
        Development,
        Homologation,
        Production
    }
}
