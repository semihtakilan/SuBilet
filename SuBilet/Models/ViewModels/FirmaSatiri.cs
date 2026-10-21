namespace SuBilet.Models.ViewModels;

// Firma listesinde her satır: firma + otobüs ve sefer sayısı (tek sorguda hesaplanır)
public record FirmaSatiri(Firma Firma, int OtobusSayisi, int SeferSayisi);
