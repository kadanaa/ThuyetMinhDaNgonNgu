# 🌍 Thuyết Minh Dạo Ngôn Ngữ (Multilingual Tour Guide)

**A comprehensive solution for tourist navigation and POI exploration with multilingual support and text-to-speech functionality.**

---

## 📱 Project Overview

**Thuyết Minh Dạo Ngôn Ngữ** is a complete system consisting of three interconnected applications:

### 1️⃣ **Tourist App** (.NET MAUI - Android)
A mobile application for tourists to discover Points of Interest (POI) with:
- 🗺️ Google Maps integration
- 📍 Current location selection
- 🔍 POI discovery based on proximity
- 🌐 Multilingual support (10 languages)
- 🔊 Text-to-speech narration
- 📝 POI information with translations

### 2️⃣ **Admin App** (Windows Form)
Administrative dashboard for managing POIs and approvals:
- 🔐 Admin authentication
- 📋 POI management (CRUD operations)
- ✅ POI approval workflow
- 👥 User management
- 📊 Dashboard statistics
- 📝 Activity logs

### 3️⃣ **POI Owner App** (Windows Form)
Application for POI owners to manage their locations:
- 📝 POI registration requests
- ✏️ POI information editing
- 📍 Location management
- 📊 Status tracking
- 🔔 Approval notifications

---

## 🗄️ Database Architecture

### Core Components
- **SQL Server Database**: ThuyetMinhDaNgonNgu
- **8 Main Tables**: Users, POI, Translations, Approvals, Media, Languages, Audit, Preferences
- **20+ Stored Procedures**: For all CRUD operations and business logic
- **10 Supported Languages**: EN, VI, ES, FR, DE, JA, ZH, KO, TH, ID

### Key Features
✅ User authentication & authorization  
✅ Geographic POI queries (Haversine formula)  
✅ Multi-language translation management  
✅ Approval workflow system  
✅ Complete audit logging  
✅ Media management  

---

## 🚀 Quick Start

### 1. Setup Database (2 minutes)
```bash
# Open SQL Server Management Studio and run:
# File > Open > ThuyetMinhDb.sql
# Then Execute > StoredProcedures.sql
```

**Or via Command Line:**
```powershell
sqlcmd -S YOUR_SERVER -i Database/ThuyetMinhDb.sql
sqlcmd -S YOUR_SERVER -d ThuyetMinhDaNgonNgu -i Database/StoredProcedures.sql
```

### 2. Update Connection Strings (1 minute)
Each project needs `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "ThuyetMinhDb": "Server=YOUR_SERVER;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

### 3. Verify Installation (1 minute)
```sql
USE ThuyetMinhDaNgonNgu;
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES; -- Should return 8
EXEC sp_GetDashboardStats;
```

✅ **Done!** Database is ready for development.

---

## 📚 Documentation

| Document | Purpose | Read Time |
|----------|---------|-----------|
| **QUICK_START.md** | 5-minute setup guide | 5 min |
| **IMPLEMENTATION_GUIDE.md** | Complete implementation guide | 30 min |
| **API_REFERENCE.md** | API documentation & code examples | Reference |
| **Database/README_DATABASE.md** | Database structure details | Reference |
| **PROJECT_STRUCTURE.md** | File organization & overview | 10 min |
| **SUMMARY.md** | Project completion summary | 5 min |

👉 **Start with QUICK_START.md** if you're new!

---

## 🔐 Default Accounts

| Role | Username | Password | Email |
|------|----------|----------|-------|
| Admin | admin | Admin@123 | admin@thuyetminh.vn |
| POI Owner | poiowner01 | Admin@123 | owner1@thuyetminh.vn |

⚠️ Change passwords in production!

---

## 📊 Sample Data

Database includes 3 pre-configured POIs:
1. **Phở Bắc Hà** (Restaurant) - 21.028511, 105.854100
2. **Cafe Trời Xanh** (Cafe) - 21.027500, 105.853800
3. **Tạp Hóa Minh Phúc** (Shop) - 21.029000, 105.855200

Plus 10 supported languages and full audit logs.

---

## 🌐 Supported Languages

- 🇬🇧 English (en)
- 🇻🇳 Vietnamese (vi)
- 🇪🇸 Spanish (es)
- 🇫🇷 French (fr)
- 🇩🇪 German (de)
- 🇯🇵 Japanese (ja)
- 🇨🇳 Chinese (zh)
- 🇰🇷 Korean (ko)
- 🇹🇭 Thai (th)
- 🇮🇩 Indonesian (id)

---

## 📁 Project Structure

```
ThuyetMinhDaNgonNgu/
│
├── 📂 Admin/                    # Admin Windows Form App
├── 📂 POIOwner/                 # POI Owner Windows Form App
├── 📂 Tourist/                  # Tourist MAUI App
│
├── 📂 Database/
│   ├── ThuyetMinhDb.sql         # Main database script
│   ├── StoredProcedures.sql     # 20+ stored procedures
│   ├── EntityModels.cs          # C# Entity models
│   ├── appsettings.template.json # Connection template
│   └── README_DATABASE.md       # Database documentation
│
├── 📄 QUICK_START.md            # 5-minute setup
├── 📄 IMPLEMENTATION_GUIDE.md    # Implementation guide
├── 📄 API_REFERENCE.md          # API documentation
├── 📄 PROJECT_STRUCTURE.md      # Project overview
├── 📄 SUMMARY.md                # Completion summary
└── 📄 README.md                 # This file
```

---

## 🛠️ Technologies Used

### Database
- **SQL Server 2016+** with advanced spatial queries
- **Entity Framework Core** ORM support
- **Stored Procedures** for business logic

### Admin & POI Owner
- **.NET Framework / .NET Core**
- **Windows Forms** (WinForms)
- **Entity Framework** or ADO.NET
- **BCrypt** for password hashing

### Tourist App
- **.NET MAUI** (Multi-platform UI)
- **Google Maps API** integration
- **Translation API** (Google Translate or Azure)
- **Text-to-Speech** (Platform-native)

---

## 📖 Key Features

### ✅ Implemented
- Complete database schema with 8 tables
- 20+ stored procedures
- Entity Framework models
- Authentication system
- Approval workflow
- Multi-language support
- Audit logging
- Sample data

### 🔄 In Development (Project Phase)
- Admin UI/Dashboard
- POI Owner registration form
- Tourist map interface
- Google Maps integration
- Translation integration
- Text-to-speech implementation

### 🎯 Future Enhancements
- User reviews & ratings
- Booking system
- Offline POI data sync
- Advanced analytics
- Mobile push notifications

---

## 🔒 Security Features

### Implemented
✅ Password hashing (BCrypt ready)  
✅ User role-based access control  
✅ Audit logging for all operations  
✅ SQL injection prevention (parameterized queries)  
✅ Input validation (database constraints)  

### Recommended for Production
- Implement JWT authentication
- Use HTTPS/TLS for API calls
- Add API rate limiting
- Implement encryption for sensitive data
- Regular security audits

---

## 🚦 Getting Started

### For Frontend Developers
1. Read: QUICK_START.md
2. Set up database
3. Copy EntityModels.cs to your project
4. Open the relevant project (Admin/POIOwner/Tourist)
5. Start implementing UI

### For Backend Developers
1. Read: IMPLEMENTATION_GUIDE.md
2. Set up database & verify installation
3. Review: API_REFERENCE.md for all available procedures
4. Implement business logic
5. Create API endpoints

### For Database Administrators
1. Read: Database/README_DATABASE.md
2. Run ThuyetMinhDb.sql
3. Run StoredProcedures.sql
4. Monitor: AuditLogs table for activity
5. Maintain: Regular backups & optimization

---

## 📞 Support & Troubleshooting

### Installation Issues
See **QUICK_START.md > Troubleshooting section**

### Database Errors
See **Database/README_DATABASE.md > Troubleshooting**

### API Usage Questions
See **API_REFERENCE.md > Code Examples**

### Implementation Help
See **IMPLEMENTATION_GUIDE.md** for your specific project

---

## 📊 Database Statistics

| Item | Count |
|------|-------|
| Tables | 8 |
| Stored Procedures | 20+ |
| Views | 1 |
| Languages | 10 |
| Sample POIs | 3 |
| Default Users | 2 |
| Indexes | 15+ |

---

## 🎯 Project Goals

✅ Enable tourists to discover POIs via mobile app  
✅ Support multiple languages with translations  
✅ Allow POI owners to manage their locations  
✅ Enable admin to approve and manage all POIs  
✅ Provide text-to-speech guided tour information  
✅ Maintain complete audit trail  
✅ Scale to support thousands of users & POIs  

---

## 📈 Performance Considerations

### Database Optimization
- Haversine formula for distance calculations
- Indexed coordinates for fast geographic queries
- Connection pooling for better throughput
- Materialized views for dashboards

### Caching Strategy
- Cache translations (rarely change)
- Cache language list (static)
- Cache POI metadata
- Implement Redis for distributed caching

### Scalability
- Read replicas for reporting
- Partitioning for large POI datasets
- CDN for media files
- Async APIs for better concurrency

---

## 🧪 Testing Recommendations

### Unit Tests
- Business logic validation
- Entity model mapping
- Stored procedure logic

### Integration Tests
- Database connectivity
- CRUD operations
- Approval workflow
- Translation retrieval

### System Tests
- End-to-end user flows
- Performance under load
- Multi-language support
- Offline functionality

---

## 📝 Development Checklist

- [ ] Database setup completed
- [ ] Connection strings configured
- [ ] EntityModels.cs copied to projects
- [ ] NuGet packages installed
- [ ] Admin project authentication working
- [ ] POI management CRUD implemented
- [ ] POI Owner registration working
- [ ] Tourist map displaying POIs
- [ ] Translation system integrated
- [ ] Text-to-speech implemented
- [ ] Unit tests written
- [ ] Integration tests written
- [ ] UAT completed
- [ ] Production deployment ready

---

## 📄 License & Credits

This project is designed for educational and commercial use.

---

## 🙏 Thank You!

All infrastructure is ready! You can now start developing the three applications.

**Happy Coding! 🚀**

---

**Project Version**: 1.0  
**Created**: 2024  
**Status**: ✅ Infrastructure Complete, Ready for Development  
**Last Updated**: 2024  

For detailed information, see the documentation files above.
