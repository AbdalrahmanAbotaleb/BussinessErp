-- ============================================================
-- AI BUSINESS BRAIN — SEED DATA
-- Enterprise-level realistic data generation
-- ============================================================
USE [BussinessDB];
GO

-- ===================== ADMIN USER =====================
-- Password: admin123 (SHA256 hash)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Role])
    VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Admin');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = 'manager1')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Role])
    VALUES ('manager1', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Manager');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = 'emp1')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Role])
    VALUES ('emp1', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Employee');
END
GO

-- ===================== SUPPLIERS (30+) =====================
SET IDENTITY_INSERT [dbo].[Suppliers] ON;

INSERT INTO [dbo].[Suppliers] ([Id],[Name],[Phone],[Email],[Address]) VALUES
(1,'Alpha Electronics Co.','0501234567','alpha@suppliers.com','Industrial Zone A, Riyadh'),
(2,'Beta Furniture Ltd.','0507654321','beta@suppliers.com','King Fahd Road, Jeddah'),
(3,'Gamma Office Supplies','0501112233','gamma@suppliers.com','Al Olaya District, Riyadh'),
(4,'Delta Food Distribution','0504445566','delta@suppliers.com','Al Khobar Port Area'),
(5,'Epsilon Chemicals','0509998877','epsilon@suppliers.com','Yanbu Industrial City'),
(6,'Zeta Textiles Inc.','0502223344','zeta@suppliers.com','Dammam Textile Hub'),
(7,'Eta Hardware Store','0505556677','eta@suppliers.com','Tabuk Commercial Zone'),
(8,'Theta Pharma Supply','0508889900','theta@suppliers.com','Medina Health District'),
(9,'Iota Auto Parts','0503334455','iota@suppliers.com','Mecca Industrial Area'),
(10,'Kappa Plastics Mfg.','0506667788','kappa@suppliers.com','Jubail Petrochemical Zone'),
(11,'Lambda Steel Works','0501239876','lambda@suppliers.com','Ras Al Khair Steel City'),
(12,'Mu Agricultural Co.','0504561234','mu@suppliers.com','Al Qassim Farm Region'),
(13,'Nu Paper Products','0507891234','nu@suppliers.com','Riyadh Paper Mill Rd'),
(14,'Xi Logistics Supply','0502345678','xi@suppliers.com','Dammam Port Free Zone'),
(15,'Omicron IT Solutions','0505678901','omicron@suppliers.com','KAUST Tech Valley'),
(16,'Pi Building Materials','0508901234','pi@suppliers.com','Abha Construction Hub'),
(17,'Rho Cleaning Products','0501234890','rho@suppliers.com','Taif Industrial Estate'),
(18,'Sigma Paint & Coatings','0504567890','sigma@suppliers.com','Hail Paint Factory Rd'),
(19,'Tau Safety Equipment','0507890123','tau@suppliers.com','Najran Safety Zone'),
(20,'Upsilon Glass Works','0502341567','upsilon@suppliers.com','Al Ahsa Glass District'),
(21,'Phi Packaging Co.','0505674890','phi@suppliers.com','Riyadh Packaging Hub'),
(22,'Chi Medical Devices','0508907123','chi@suppliers.com','Jeddah Health Tech Park'),
(23,'Psi Solar Energy','0501237890','psi@suppliers.com','NEOM Green Zone'),
(24,'Omega Food Import','0504560123','omega@suppliers.com','Jeddah Port Area'),
(25,'Atlas Heavy Machinery','0507893456','atlas@suppliers.com','Yanbu Heavy Industry'),
(26,'Titan Tools Corp.','0502346789','titan@suppliers.com','Riyadh Tool Market'),
(27,'Nova Lighting Co.','0505679012','nova@suppliers.com','Dammam Light District'),
(28,'Zenith Cable & Wire','0508902345','zenith@suppliers.com','Jubail Cable Zone'),
(29,'Apex Rubber Products','0501235678','apex@suppliers.com','Jeddah Rubber Works'),
(30,'Core Components Ltd.','0504568901','core@suppliers.com','Riyadh Tech Park'),
(31,'Vertex Analytics Inc.','0507891567','vertex@suppliers.com','KFUPM Innovation Hub'),
(32,'Prism Optical Supplies','0502340189','prism@suppliers.com','Dhahran Optical Center');

SET IDENTITY_INSERT [dbo].[Suppliers] OFF;
GO

-- ===================== EMPLOYEES (50+) =====================
SET IDENTITY_INSERT [dbo].[Employees] ON;

INSERT INTO [dbo].[Employees] ([Id],[Name],[Phone],[Salary],[HireDate],[Department]) VALUES
(1,'Mohammed Al-Rashid','0551001001',12000.00,'2020-01-15','Management'),
(2,'Fatima Al-Zahrani','0551001002',9500.00,'2020-03-20','Accounting'),
(3,'Ahmed Al-Otaibi','0551001003',8000.00,'2020-06-10','Sales'),
(4,'Sara Al-Dossari','0551001004',8500.00,'2020-07-01','HR'),
(5,'Khalid Al-Ghamdi','0551001005',7500.00,'2020-09-15','Warehouse'),
(6,'Nora Al-Shehri','0551001006',9000.00,'2020-11-20','Accounting'),
(7,'Omar Al-Malki','0551001007',7000.00,'2021-01-10','Sales'),
(8,'Huda Al-Harbi','0551001008',7500.00,'2021-02-25','Customer Service'),
(9,'Youssef Al-Qahtani','0551001009',8000.00,'2021-04-15','IT'),
(10,'Layla Al-Shahrani','0551001010',7000.00,'2021-05-30','Sales'),
(11,'Faisal Al-Mutairi','0551001011',11000.00,'2019-01-10','Management'),
(12,'Maha Al-Subaie','0551001012',8500.00,'2021-07-01','Marketing'),
(13,'Abdulaziz Al-Dosari','0551001013',7500.00,'2021-08-15','Warehouse'),
(14,'Reem Al-Tamimi','0551001014',8000.00,'2021-09-20','Accounting'),
(15,'Hassan Al-Zahrani','0551001015',7000.00,'2021-11-05','Sales'),
(16,'Aisha Al-Otaibi','0551001016',7500.00,'2022-01-10','Customer Service'),
(17,'Bandar Al-Shammari','0551001017',9500.00,'2019-06-15','IT'),
(18,'Dalal Al-Enezi','0551001018',7000.00,'2022-03-20','HR'),
(19,'Turki Al-Dossary','0551001019',7500.00,'2022-05-01','Sales'),
(20,'Nouf Al-Rashidi','0551001020',8000.00,'2022-06-15','Marketing'),
(21,'Saad Al-Otaibi','0551001021',7000.00,'2022-07-20','Warehouse'),
(22,'Jawahir Al-Subaie','0551001022',7500.00,'2022-08-10','Accounting'),
(23,'Majed Al-Qahtani','0551001023',8500.00,'2022-09-25','Sales'),
(24,'Mona Al-Harbi','0551001024',7000.00,'2022-10-15','Customer Service'),
(25,'Nasser Al-Ghamdi','0551001025',7500.00,'2022-11-30','IT'),
(26,'Haifa Al-Malki','0551001026',7000.00,'2023-01-10','HR'),
(27,'Sultan Al-Shehri','0551001027',8000.00,'2023-02-20','Sales'),
(28,'Abeer Al-Mutairi','0551001028',7500.00,'2023-03-15','Warehouse'),
(29,'Waleed Al-Zahrani','0551001029',9000.00,'2023-04-01','Management'),
(30,'Dana Al-Shahrani','0551001030',7000.00,'2023-05-20','Marketing'),
(31,'Ibrahim Al-Dosari','0551001031',7500.00,'2023-06-10','Accounting'),
(32,'Lama Al-Tamimi','0551001032',8000.00,'2023-07-25','Sales'),
(33,'Fahad Al-Otaibi','0551001033',7000.00,'2023-08-15','Customer Service'),
(34,'Ghada Al-Enezi','0551001034',7500.00,'2023-09-01','IT'),
(35,'Mansour Al-Shammari','0551001035',8500.00,'2023-10-20','Sales'),
(36,'Hanan Al-Rashidi','0551001036',7000.00,'2023-11-10','HR'),
(37,'Talal Al-Subaie','0551001037',7500.00,'2024-01-15','Warehouse'),
(38,'Amira Al-Qahtani','0551001038',8000.00,'2024-02-25','Marketing'),
(39,'Badr Al-Harbi','0551001039',7000.00,'2024-03-10','Sales'),
(40,'Salwa Al-Ghamdi','0551001040',7500.00,'2024-04-20','Accounting'),
(41,'Ziad Al-Malki','0551001041',9500.00,'2024-05-01','IT'),
(42,'Rania Al-Shehri','0551001042',7000.00,'2024-06-15','Customer Service'),
(43,'Nawaf Al-Mutairi','0551001043',7500.00,'2024-07-25','Sales'),
(44,'Maryam Al-Zahrani','0551001044',8000.00,'2024-08-10','HR'),
(45,'Sami Al-Shahrani','0551001045',7000.00,'2024-09-20','Warehouse'),
(46,'Hessa Al-Dosari','0551001046',7500.00,'2024-10-05','Marketing'),
(47,'Abdullah Al-Tamimi','0551001047',8500.00,'2024-11-15','Management'),
(48,'Wafa Al-Otaibi','0551001048',7000.00,'2024-12-01','Sales'),
(49,'Hamad Al-Enezi','0551001049',7500.00,'2025-01-10','Accounting'),
(50,'Samar Al-Shammari','0551001050',7000.00,'2025-02-15','Customer Service'),
(51,'Rashid Al-Rashidi','0551001051',8000.00,'2025-01-20','IT'),
(52,'Latifa Al-Subaie','0551001052',7500.00,'2025-02-10','Sales');

SET IDENTITY_INSERT [dbo].[Employees] OFF;
GO

-- ===================== CUSTOMERS (300+) =====================
-- Generated using name patterns
DECLARE @i INT = 1;
DECLARE @firstNames TABLE (Id INT IDENTITY(1,1), Name NVARCHAR(50));
INSERT INTO @firstNames(Name) VALUES
('Ali'),('Omar'),('Youssef'),('Hassan'),('Khaled'),('Saud'),('Nasser'),('Hamad'),
('Fahad'),('Saleh'),('Ibrahim'),('Waleed'),('Turki'),('Bandar'),('Faisal'),('Majed'),
('Saad'),('Sultan'),('Rashid'),('Ziad'),('Mansour'),('Badr'),('Talal'),('Nawaf'),
('Sami'),('Abdulaziz'),('Mohammed'),('Ahmad'),('Tariq'),('Bilal'),
('Fatima'),('Sara'),('Nora'),('Huda'),('Layla'),('Maha'),('Reem'),('Aisha'),
('Dalal'),('Nouf'),('Jawahir'),('Mona'),('Haifa'),('Abeer'),('Dana'),('Lama'),
('Ghada'),('Hanan'),('Amira'),('Salwa'),('Rania'),('Maryam'),('Hessa'),('Wafa'),
('Samar'),('Latifa'),('Khadija'),('Zainab'),('Arwa'),('Bushra');

DECLARE @lastNames TABLE (Id INT IDENTITY(1,1), Name NVARCHAR(50));
INSERT INTO @lastNames(Name) VALUES
('Al-Rashid'),('Al-Zahrani'),('Al-Otaibi'),('Al-Dossari'),('Al-Ghamdi'),
('Al-Shehri'),('Al-Malki'),('Al-Harbi'),('Al-Qahtani'),('Al-Shahrani'),
('Al-Mutairi'),('Al-Subaie'),('Al-Tamimi'),('Al-Enezi'),('Al-Shammari'),
('Al-Rashidi'),('Al-Dosary'),('Al-Juhani'),('Al-Anazi'),('Al-Yami');

DECLARE @cities TABLE (Id INT IDENTITY(1,1), Name NVARCHAR(50));
INSERT INTO @cities(Name) VALUES
('Riyadh'),('Jeddah'),('Dammam'),('Mecca'),('Medina'),('Tabuk'),('Abha'),('Hail'),
('Najran'),('Al Khobar'),('Jubail'),('Yanbu'),('Al Qassim'),('Taif'),('Dhahran');

DECLARE @fCount INT = (SELECT COUNT(*) FROM @firstNames);
DECLARE @lCount INT = (SELECT COUNT(*) FROM @lastNames);
DECLARE @cCount INT = (SELECT COUNT(*) FROM @cities);
DECLARE @fname NVARCHAR(50), @lname NVARCHAR(50), @city NVARCHAR(50);

WHILE @i <= 310
BEGIN
    SELECT @fname = Name FROM @firstNames WHERE Id = ((@i - 1) % @fCount) + 1;
    SELECT @lname = Name FROM @lastNames WHERE Id = ((@i - 1) % @lCount) + 1;
    SELECT @city  = Name FROM @cities WHERE Id = ((@i - 1) % @cCount) + 1;

    INSERT INTO [dbo].[Customers] ([Name],[Phone],[Email],[Address])
    VALUES (
        @fname + ' ' + @lname,
        '055' + RIGHT('0000000' + CAST(2000000 + @i AS VARCHAR), 7),
        LOWER(@fname) + '.' + LOWER(REPLACE(@lname,'-','')) + CAST(@i AS VARCHAR) + '@email.com',
        @city + ', Street ' + CAST((@i % 50) + 1 AS VARCHAR)
    );
    SET @i = @i + 1;
END
GO

-- ===================== PRODUCTS (150+) =====================
SET IDENTITY_INSERT [dbo].[Products] ON;

INSERT INTO [dbo].[Products] ([Id],[Name],[Category],[CostPrice],[SellPrice],[Quantity],[ReorderLevel]) VALUES
-- Electronics (1-25)
(1,'Laptop Pro 15"','Electronics',2500.00,3499.99,45,10),
(2,'Desktop PC i7','Electronics',2200.00,2999.99,30,8),
(3,'Wireless Mouse','Electronics',25.00,49.99,200,50),
(4,'Mechanical Keyboard','Electronics',80.00,149.99,150,30),
(5,'27" Monitor 4K','Electronics',800.00,1199.99,35,10),
(6,'USB Hub 7-Port','Electronics',15.00,34.99,300,60),
(7,'Webcam HD 1080p','Electronics',40.00,79.99,100,25),
(8,'Bluetooth Speaker','Electronics',60.00,109.99,120,30),
(9,'Noise Cancel Headphones','Electronics',150.00,279.99,80,20),
(10,'External SSD 1TB','Electronics',90.00,159.99,100,25),
(11,'Tablet 10"','Electronics',400.00,599.99,50,15),
(12,'Smart Watch','Electronics',120.00,229.99,70,20),
(13,'Power Bank 20000mAh','Electronics',25.00,54.99,250,50),
(14,'Wireless Charger','Electronics',18.00,39.99,200,40),
(15,'HDMI Cable 2m','Electronics',5.00,14.99,500,100),
(16,'Network Router','Electronics',45.00,89.99,80,20),
(17,'UPS 1500VA','Electronics',120.00,219.99,40,10),
(18,'Surge Protector','Electronics',12.00,29.99,300,60),
(19,'Graphics Card RTX','Electronics',1200.00,1799.99,15,5),
(20,'RAM 16GB DDR5','Electronics',80.00,139.99,100,25),
(21,'CPU Processor i9','Electronics',500.00,749.99,20,5),
(22,'Motherboard ATX','Electronics',180.00,299.99,30,8),
(23,'PC Case Tower','Electronics',70.00,119.99,40,10),
(24,'Cooling Fan RGB','Electronics',15.00,34.99,200,40),
(25,'Printer MFP Laser','Electronics',350.00,549.99,25,8),
-- Office Supplies (26-55)
(26,'Copy Paper A4 (Ream)','Office Supplies',8.00,18.99,800,200),
(27,'Ballpoint Pen Box','Office Supplies',3.00,9.99,600,150),
(28,'Notebook A5 Pack','Office Supplies',5.00,14.99,400,100),
(29,'Stapler Heavy Duty','Office Supplies',8.00,19.99,150,30),
(30,'Staples Box 5000','Office Supplies',2.00,5.99,500,100),
(31,'Paper Clips Box','Office Supplies',1.50,4.99,600,120),
(32,'Binder Clips Set','Office Supplies',3.00,8.99,400,80),
(33,'Whiteboard Marker Set','Office Supplies',6.00,14.99,300,60),
(34,'Permanent Marker Set','Office Supplies',5.00,12.99,350,70),
(35,'Highlighter Set','Office Supplies',4.00,10.99,400,80),
(36,'Correction Tape','Office Supplies',2.00,5.99,500,100),
(37,'Scissors Office','Office Supplies',3.00,8.99,200,40),
(38,'Tape Dispenser','Office Supplies',4.00,11.99,250,50),
(39,'File Folder Set','Office Supplies',6.00,15.99,300,60),
(40,'Desk Organizer','Office Supplies',12.00,29.99,100,25),
(41,'Calculator Scientific','Office Supplies',15.00,34.99,150,30),
(42,'Rubber Bands Box','Office Supplies',1.00,3.99,600,120),
(43,'Envelope Pack','Office Supplies',4.00,10.99,400,80),
(44,'Label Maker','Office Supplies',30.00,59.99,60,15),
(45,'Whiteboard 90x60','Office Supplies',40.00,79.99,30,8),
(46,'Corkboard 60x40','Office Supplies',20.00,44.99,40,10),
(47,'Laminating Pouches','Office Supplies',8.00,19.99,200,40),
(48,'Stamp Pad Ink','Office Supplies',3.00,7.99,300,60),
(49,'Desktop Calendar','Office Supplies',5.00,13.99,200,40),
(50,'Sticky Notes Pack','Office Supplies',3.00,8.99,500,100),
(51,'Paper Shredder','Office Supplies',80.00,149.99,20,5),
(52,'Desk Lamp LED','Office Supplies',25.00,49.99,80,20),
(53,'Chair Mat','Office Supplies',30.00,59.99,40,10),
(54,'Monitor Stand','Office Supplies',20.00,44.99,60,15),
(55,'Cable Management Kit','Office Supplies',10.00,24.99,100,25),
-- Furniture (56-75)
(56,'Office Chair Ergonomic','Furniture',300.00,549.99,25,5),
(57,'Executive Desk','Furniture',400.00,749.99,15,4),
(58,'Conference Table 8P','Furniture',600.00,1099.99,8,2),
(59,'Filing Cabinet 4D','Furniture',150.00,299.99,20,5),
(60,'Bookshelf 5-Tier','Furniture',120.00,229.99,25,5),
(61,'Reception Desk','Furniture',500.00,899.99,5,2),
(62,'Visitor Chair','Furniture',80.00,159.99,40,10),
(63,'Storage Locker','Furniture',90.00,179.99,15,4),
(64,'Standing Desk','Furniture',350.00,649.99,12,3),
(65,'Drafting Table','Furniture',200.00,399.99,8,2),
(66,'Corner Desk','Furniture',250.00,449.99,10,3),
(67,'Meeting Room Chair','Furniture',100.00,199.99,30,8),
(68,'TV Stand','Furniture',80.00,159.99,15,4),
(69,'Coat Rack','Furniture',25.00,54.99,20,5),
(70,'Partition Screen','Furniture',70.00,139.99,20,5),
(71,'Kitchen Table Staff','Furniture',150.00,289.99,5,2),
(72,'Lounge Sofa 3P','Furniture',500.00,949.99,6,2),
(73,'Outdoor Bench','Furniture',100.00,199.99,10,3),
(74,'Adjustable Shelf Unit','Furniture',60.00,119.99,18,5),
(75,'Pedestal Drawer','Furniture',50.00,99.99,25,6),
-- Cleaning (76-95)
(76,'Floor Cleaner 5L','Cleaning',8.00,18.99,200,40),
(77,'Glass Cleaner 1L','Cleaning',4.00,10.99,300,60),
(78,'Hand Soap 5L','Cleaning',6.00,14.99,250,50),
(79,'Disinfectant Spray','Cleaning',5.00,12.99,300,60),
(80,'Trash Bags 50pc','Cleaning',4.00,9.99,400,80),
(81,'Mop & Bucket Set','Cleaning',15.00,34.99,60,15),
(82,'Broom Industrial','Cleaning',8.00,19.99,80,20),
(83,'Dust Pan Set','Cleaning',5.00,12.99,100,25),
(84,'Paper Towel Roll','Cleaning',3.00,7.99,500,100),
(85,'Toilet Paper 24pk','Cleaning',8.00,17.99,300,60),
(86,'Air Freshener','Cleaning',3.00,8.99,400,80),
(87,'Rubber Gloves Box','Cleaning',5.00,12.99,250,50),
(88,'Microfiber Cloth 10pk','Cleaning',6.00,14.99,200,40),
(89,'Vacuum Cleaner','Cleaning',200.00,379.99,15,4),
(90,'Pressure Washer','Cleaning',300.00,549.99,8,2),
(91,'Carpet Cleaner 5L','Cleaning',10.00,24.99,100,25),
(92,'Stainless Cleaner','Cleaning',5.00,11.99,200,40),
(93,'Drain Cleaner','Cleaning',4.00,9.99,250,50),
(94,'Bleach 5L','Cleaning',5.00,11.99,300,60),
(95,'Wet Floor Sign','Cleaning',8.00,17.99,40,10),
-- Safety (96-115)
(96,'Hard Hat','Safety',10.00,24.99,100,25),
(97,'Safety Vest','Safety',8.00,19.99,150,30),
(98,'Safety Goggles','Safety',6.00,14.99,200,40),
(99,'First Aid Kit','Safety',25.00,54.99,50,10),
(100,'Fire Extinguisher','Safety',40.00,89.99,30,8),
(101,'Safety Boots','Safety',50.00,99.99,60,15),
(102,'Ear Protection','Safety',8.00,19.99,120,30),
(103,'Face Shield','Safety',12.00,27.99,80,20),
(104,'Safety Gloves Pair','Safety',4.00,10.99,300,60),
(105,'Warning Tape Roll','Safety',3.00,8.99,200,40),
(106,'Emergency Light','Safety',30.00,64.99,25,6),
(107,'Smoke Detector','Safety',15.00,34.99,40,10),
(108,'Safety Harness','Safety',60.00,119.99,20,5),
(109,'Reflective Stickers','Safety',2.00,5.99,400,80),
(110,'Chemical Spill Kit','Safety',80.00,159.99,10,3),
(111,'Fire Blanket','Safety',15.00,34.99,30,8),
(112,'Eye Wash Station','Safety',20.00,44.99,15,4),
(113,'Dust Mask Box','Safety',5.00,12.99,300,60),
(114,'Safety Cone Set','Safety',10.00,24.99,50,12),
(115,'Lock Out Tag Out Kit','Safety',25.00,54.99,20,5),
-- Food & Beverages (116-135)
(116,'Coffee Beans 1kg','Food & Beverages',20.00,44.99,150,30),
(117,'Tea Bags 100pk','Food & Beverages',8.00,18.99,200,40),
(118,'Sugar 5kg','Food & Beverages',6.00,13.99,250,50),
(119,'Creamer 500g','Food & Beverages',5.00,11.99,200,40),
(120,'Bottled Water 24pk','Food & Beverages',8.00,17.99,300,60),
(121,'Juice Box 12pk','Food & Beverages',10.00,22.99,150,30),
(122,'Biscuit Variety Box','Food & Beverages',6.00,14.99,200,40),
(123,'Instant Noodle 30pk','Food & Beverages',12.00,27.99,100,25),
(124,'Canned Tuna 12pk','Food & Beverages',15.00,32.99,120,30),
(125,'Rice 10kg','Food & Beverages',18.00,38.99,100,25),
(126,'Cooking Oil 5L','Food & Beverages',10.00,22.99,150,30),
(127,'Salt 1kg','Food & Beverages',1.00,3.99,400,80),
(128,'Pepper Ground 250g','Food & Beverages',3.00,7.99,300,60),
(129,'Tomato Paste 12pk','Food & Beverages',8.00,18.99,200,40),
(130,'Milk UHT 12pk','Food & Beverages',12.00,26.99,150,30),
(131,'Honey 500g','Food & Beverages',15.00,34.99,80,20),
(132,'Dates 1kg Premium','Food & Beverages',10.00,24.99,200,40),
(133,'Mixed Nuts 500g','Food & Beverages',12.00,27.99,100,25),
(134,'Chocolate Bar 24pk','Food & Beverages',14.00,29.99,150,30),
(135,'Energy Drink 24pk','Food & Beverages',20.00,42.99,100,25),
-- Packaging (136-150)
(136,'Cardboard Box Small','Packaging',1.00,3.49,500,100),
(137,'Cardboard Box Medium','Packaging',2.00,5.49,400,80),
(138,'Cardboard Box Large','Packaging',3.00,7.99,300,60),
(139,'Packing Tape Roll','Packaging',2.00,5.99,600,120),
(140,'Bubble Wrap 10m','Packaging',5.00,12.99,200,40),
(141,'Stretch Film Roll','Packaging',8.00,18.99,150,30),
(142,'Packing Peanuts 50L','Packaging',6.00,14.99,100,25),
(143,'Padded Envelope 50pk','Packaging',10.00,22.99,150,30),
(144,'Shipping Labels 500','Packaging',8.00,17.99,200,40),
(145,'Poly Bags 100pk','Packaging',3.00,7.99,400,80),
(146,'Gift Box Set','Packaging',5.00,12.99,100,25),
(147,'Paper Bag 100pk','Packaging',4.00,10.99,300,60),
(148,'Tissue Paper Pack','Packaging',2.00,5.99,400,80),
(149,'Ribbon Roll','Packaging',3.00,7.99,200,40),
(150,'Foam Sheet Pack','Packaging',4.00,10.99,150,30),
(151,'Shrink Wrap 100m','Packaging',7.00,15.99,100,25),
(152,'Pallets Wooden','Packaging',15.00,34.99,40,10),
(153,'Strapping Tape','Packaging',6.00,13.99,200,40),
(154,'Anti-Static Bags 50pk','Packaging',8.00,19.99,120,25),
(155,'Document Pouch 100pk','Packaging',5.00,12.99,200,40);

SET IDENTITY_INSERT [dbo].[Products] OFF;
GO

-- ===================== PURCHASES (1000+) =====================
-- Generate purchases over last 18 months
DECLARE @purchaseCount INT = 1;
DECLARE @suppCount INT = 32;
DECLARE @prodCount INT = 155;
DECLARE @pDate DATETIME;
DECLARE @pSuppId INT;
DECLARE @pProdId INT;
DECLARE @pQty INT;
DECLARE @pCost DECIMAL(18,2);
DECLARE @purchaseId INT;

WHILE @purchaseCount <= 1050
BEGIN
    -- Random date in last 18 months
    SET @pDate = DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 540, GETDATE());
    SET @pSuppId = (ABS(CHECKSUM(NEWID())) % @suppCount) + 1;

    INSERT INTO [dbo].[Purchases] ([SupplierId],[Date],[TotalAmount])
    VALUES (@pSuppId, @pDate, 0);
    SET @purchaseId = SCOPE_IDENTITY();

    -- 1-4 items per purchase
    DECLARE @itemCount INT = (ABS(CHECKSUM(NEWID())) % 4) + 1;
    DECLARE @itemIdx INT = 1;
    DECLARE @purchaseTotal DECIMAL(18,2) = 0;

    WHILE @itemIdx <= @itemCount
    BEGIN
        SET @pProdId = (ABS(CHECKSUM(NEWID())) % @prodCount) + 1;
        SET @pQty = (ABS(CHECKSUM(NEWID())) % 50) + 5;
        SELECT @pCost = [CostPrice] FROM [dbo].[Products] WHERE [Id] = @pProdId;

        IF @pCost IS NOT NULL
        BEGIN
            INSERT INTO [dbo].[PurchaseItems] ([PurchaseId],[ProductId],[Quantity],[CostPrice])
            VALUES (@purchaseId, @pProdId, @pQty, @pCost);
            SET @purchaseTotal = @purchaseTotal + (@pQty * @pCost);
        END
        SET @itemIdx = @itemIdx + 1;
    END

    UPDATE [dbo].[Purchases] SET [TotalAmount] = @purchaseTotal WHERE [Id] = @purchaseId;
    SET @purchaseCount = @purchaseCount + 1;
END
GO

-- ===================== SALES (2000+) =====================
-- Generate sales over last 18 months with seasonal variance
DECLARE @saleCount INT = 1;
DECLARE @custCount INT = (SELECT COUNT(*) FROM [dbo].[Customers]);
DECLARE @prodCnt INT = 155;
DECLARE @sDate DATETIME;
DECLARE @sCustId INT;
DECLARE @sProdId INT;
DECLARE @sQty INT;
DECLARE @sPrice DECIMAL(18,2);
DECLARE @saleId INT;

WHILE @saleCount <= 2200
BEGIN
    -- Random date in last 18 months with seasonal weighting
    SET @sDate = DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 540, GETDATE());

    -- Boost sales in months 11,12,1 (holiday season) by adding more recent dates
    IF @saleCount % 5 = 0
        SET @sDate = DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 90, GETDATE());

    SET @sCustId = (ABS(CHECKSUM(NEWID())) % @custCount) + 1;

    INSERT INTO [dbo].[Sales] ([CustomerId],[Date],[TotalAmount])
    VALUES (@sCustId, @sDate, 0);
    SET @saleId = SCOPE_IDENTITY();

    -- 1-5 items per sale
    DECLARE @sItemCount INT = (ABS(CHECKSUM(NEWID())) % 5) + 1;
    DECLARE @sItemIdx INT = 1;
    DECLARE @saleTotal DECIMAL(18,2) = 0;

    WHILE @sItemIdx <= @sItemCount
    BEGIN
        SET @sProdId = (ABS(CHECKSUM(NEWID())) % @prodCnt) + 1;
        SET @sQty = (ABS(CHECKSUM(NEWID())) % 10) + 1;
        SELECT @sPrice = [SellPrice] FROM [dbo].[Products] WHERE [Id] = @sProdId;

        IF @sPrice IS NOT NULL
        BEGIN
            INSERT INTO [dbo].[SaleItems] ([SaleId],[ProductId],[Quantity],[SellPrice])
            VALUES (@saleId, @sProdId, @sQty, @sPrice);
            SET @saleTotal = @saleTotal + (@sQty * @sPrice);
        END
        SET @sItemIdx = @sItemIdx + 1;
    END

    UPDATE [dbo].[Sales] SET [TotalAmount] = @saleTotal WHERE [Id] = @saleId;
    SET @saleCount = @saleCount + 1;
END
GO

-- ===================== EXPENSES =====================
-- Multiple categories over 18 months
DECLARE @expMonth INT = 1;
DECLARE @expYear INT;
DECLARE @expDate DATETIME;
DECLARE @baseDate DATETIME = DATEADD(MONTH, -18, GETDATE());

WHILE @expMonth <= 18
BEGIN
    SET @expDate = DATEADD(MONTH, @expMonth - 1, @baseDate);
    SET @expYear = YEAR(@expDate);

    -- Rent
    INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
    VALUES ('Office Rent', 15000.00, 'Rent', @expDate);

    -- Utilities
    INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
    VALUES ('Electricity Bill', 2500.00 + (ABS(CHECKSUM(NEWID())) % 1500), 'Utilities', @expDate);

    INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
    VALUES ('Water Bill', 500.00 + (ABS(CHECKSUM(NEWID())) % 300), 'Utilities', @expDate);

    INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
    VALUES ('Internet Service', 800.00, 'Utilities', @expDate);

    -- Salaries (aggregated)
    INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
    VALUES ('Staff Salaries', 180000.00 + (ABS(CHECKSUM(NEWID())) % 20000), 'Salaries', @expDate);

    -- Marketing (varies)
    INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
    VALUES ('Digital Marketing', 3000.00 + (ABS(CHECKSUM(NEWID())) % 5000), 'Marketing', @expDate);

    -- Maintenance
    INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
    VALUES ('Building Maintenance', 1000.00 + (ABS(CHECKSUM(NEWID())) % 2000), 'Maintenance', @expDate);

    -- Insurance (quarterly)
    IF @expMonth % 3 = 1
    BEGIN
        INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
        VALUES ('Business Insurance', 8000.00, 'Insurance', @expDate);
    END

    -- Training (bi-monthly)
    IF @expMonth % 2 = 0
    BEGIN
        INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
        VALUES ('Staff Training', 2000.00 + (ABS(CHECKSUM(NEWID())) % 3000), 'Training', @expDate);
    END

    -- Miscellaneous
    INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
    VALUES ('Office Supplies Purchase', 500.00 + (ABS(CHECKSUM(NEWID())) % 1000), 'Miscellaneous', @expDate);

    INSERT INTO [dbo].[Expenses] ([Title],[Amount],[Category],[Date])
    VALUES ('Transportation', 1500.00 + (ABS(CHECKSUM(NEWID())) % 1000), 'Transportation', @expDate);

    SET @expMonth = @expMonth + 1;
END
GO

PRINT '=== SEED DATA COMPLETE ==='
GO
