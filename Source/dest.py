# (island_id, id, name, region, lat, lon)
DEST = [
 ("sumatera","toba","Danau Toba","Sumatera Utara",2.65,98.88),
 ("sumatera","jamgadang","Jam Gadang","Bukittinggi, Sumatera Barat",-0.305,100.369),
 ("sumatera","ampera","Jembatan Ampera","Palembang, Sumatera Selatan",-2.99,104.76),
 ("jawa","kotatua","Kota Tua Jakarta","DKI Jakarta",-6.135,106.813),
 ("jawa","borobudur","Candi Borobudur","Magelang, Jawa Tengah",-7.608,110.204),
 ("jawa","bromo","Gunung Bromo","Jawa Timur",-7.942,112.953),
 ("kalimantan","khatulistiwa","Tugu Khatulistiwa","Pontianak, Kalimantan Barat",0.0,109.33),
 ("kalimantan","tanjungputing","Tanjung Puting","Kalimantan Tengah",-2.78,111.8),
 ("kalimantan","mahakam","Sungai Mahakam","Samarinda, Kalimantan Timur",-0.5,117.15),
 ("balinusra","tanahlot","Pura Tanah Lot","Tabanan, Bali",-8.544,115.188),
 ("balinusra","rinjani","Gunung Rinjani","Lombok, Nusa Tenggara Barat",-8.411,116.457),
 ("balinusra","labuanbajo","Labuan Bajo","Flores, Nusa Tenggara Timur",-8.497,119.888),
 ("sulawesi","makassar","Benteng Rotterdam","Makassar, Sulawesi Selatan",-5.134,119.406),
 ("sulawesi","toraja","Tana Toraja","Sulawesi Selatan",-3.05,119.85),
 ("sulawesi","bunaken","Taman Laut Bunaken","Manado, Sulawesi Utara",1.47,124.84),
 ("maluku","ternate","Ternate","Maluku Utara",0.664,127.633),
 ("maluku","ambon","Kota Ambon","Maluku",-3.695,128.18),
 ("maluku","banda","Banda Neira","Kepulauan Banda, Maluku",-4.52,129.9),
 ("papua","rajaampat","Raja Ampat","Papua Barat Daya",-0.172,130.549),
 ("papua","baliem","Lembah Baliem","Wamena, Papua",-4.09,138.94),
 ("papua","sentani","Danau Sentani","Jayapura, Papua",-2.62,140.52),
]
BX=(909.1-90.9)/(138.6793809294138-97.31095197695628)
BY=(33.4-334.2)/(5.068465449348689+10.080985198323209)
def svg(lat,lon): return (90.9+BX*(lon-97.31095197695628), 334.2+BY*(lat+10.080985198323209))
