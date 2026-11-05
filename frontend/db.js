// הוספת צימר חדש דרך ה-API (דורש הרשאת מנהל)
async function adminAddCottage(cottageData) {
    try {
        await apiRequest("/cottages", {
            method: "POST",
            body: JSON.stringify({
                name: cottageData.name,
                location: cottageData.location,
                pricePerNight: Number(cottageData.price),
                maxGuests: Number(cottageData.guests)
            })
        });
        alert('הצימר נוסף בהצלחה למסד הנתונים!');
        window.location.reload();
    } catch (error) {
        console.error('שגיאה בהוספת צימר:', error);
        alert('שגיאה בהוספת הצימר. ודאי שאת מחוברת כמנהלת מערכת.');
    }
}

// מחיקת צימר דרך ה-API (דורש הרשאת מנהל)
async function adminDeleteCottage(id) {
    try {
        await apiRequest(`/cottages/${id}`, {
            method: "DELETE"
        });
        alert('הצימר נמחק בהצלחה!');
        window.location.reload();
    } catch (error) {
        console.error('שגיאה במחיקת צימר:', error);
        alert('שגיאה במחיקת הצימר (ייתכן שיש הזמנות פעילות המשויכות אליו).');
    }
}