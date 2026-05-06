# SBYS Prompt Library for Copilot Claude Sonnet 4.6

## Regel
Brug én prompt per feature. Upload kun relevante AI-context filer og relevante kodefiler.

---

# Mini prompt — analyse only

```text
Læs de uploadede SBYS context-filer og relevante kodefiler.

Lav kun analyse. Implementér ikke kode.

Beskriv:
- hvad der findes
- hvad der mangler
- hvilke filer der er relevante
- første anbefalede implementeringstrin

Vent på min godkendelse før kode.
```

---

# Mini prompt — implement first step

```text
Implementér kun det første godkendte trin.

Regler:
- Skriv hele filer.
- Brug eksisterende namespaces.
- Lav ikke andre ændringer.
- Forklar kort hvorfor ændringen er nødvendig.
- Giv test-trin bagefter.
```

---

# Mini prompt — fix build error

```text
Jeg har denne build/runtime fejl.

Læs kun de uploadede fejl og relevante filer.

Find årsagen og ret kun det nødvendige.
Returnér hele filer for ændrede filer.
Forklar kort hvad fejlen var.
```

---

# Mini prompt — migration

```text
Læs relevante entity/context filer.

Tjek om EF Core mappings er korrekte.
Foreslå migrationskommando.
Ret kun entities/context hvis nødvendigt.
Returnér hele filer.
```

---

# Mini prompt — frontend mobile view

```text
Læs relevante frontend filer.

Ret UI til mobilvisning.
Regler:
- Simpelt mobile-first layout
- Ingen nye frontend frameworks uden aftale
- Brug eksisterende styling patterns
- Returnér hele filer
```
