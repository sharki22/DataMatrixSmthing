namespace DataMatrix;

using System.Diagnostics;
using System.Text;

/// <summary>Сервис построения символа Data Matrix.</summary>
public static class DataMatrixBuilder
{
    /// <summary>Строит символ Data Matrix из текста.</summary>
    public static string ToDataMatrix(this string text, int? rows = null, int? columns = null, bool invert = false)
    {
        byte[] codewords = EncodeToCodewords(text);

        var (symbol, capacity) = SelectSymbolSize(rows, columns, codewords.Length);

        byte[] padded = PadToDataCapacity(codewords, capacity);
        byte[] withErrorCorrection = AddErrorCorrection(padded, symbol);
        uint[] moduleBitmap = BuildModuleBitmap(withErrorCorrection, symbol);

        return CreateModuleMatrix(moduleBitmap, symbol).ToMatrixString(invert);
    }

    /// <summary>Возвращает высоту и ширину символа для текста.</summary>
    public static (int Rows, int Columns) GetSymbolSize(this string text, int? rows = null, int? columns = null)
    {
        byte[] codewords = EncodeToCodewords(text);

        var (symbol, _) = SelectSymbolSize(rows, columns, codewords.Length);

        return (symbol.Rows, symbol.Columns);
    }

    /// <summary>Возвращает уровень коррекции ошибок для текста.</summary>
    public static int GetCorrectionLevel(this string text, int? rows = null, int? columns = null)
    {
        byte[] codewords = EncodeToCodewords(text);

        var (symbol, _) = SelectSymbolSize(rows, columns, codewords.Length);

        return symbol.EccCount;
    }

    #region Encoding

    /// <summary>Символ цифры '0'.</summary>
    private const char ASCII_ZERO = '0';

    /// <summary>Кодовое слово для символа расширенного ASCII.</summary>
    private const byte EXTENDED_ASCII = 235;

    /// <summary>Кодирует текст в кодовые слова.</summary>
    private static byte[] EncodeToCodewords(string text)
    {
        var buffer = new List<byte>(text.Length);

        for (var i = 0; i < text.Length; i++)
        {
            char current = text[i];

            if (!char.IsDigit(current))
            {
                buffer.AddSingleChar(current);
                continue;
            }

            if (i + 1 < text.Length && char.IsDigit(text[i + 1]))
            {
                buffer.AddDigitPair(current, text[i + 1]);
                i++;
            }
            else
            {
                /// <summary>Цифра без пары кодируется как обычный символ.</summary>
                buffer.AddSingleChar(current);
            }
        }

        return [.. buffer];
    }

    /// <summary>Добавляет кодовое слово для пары цифр.</summary>
    private static void AddDigitPair(this List<byte> buffer, char tens, char ones)
        => buffer.Add((byte)((tens - ASCII_ZERO) * 10 + (ones - ASCII_ZERO) + 130));

    /// <summary>Добавляет кодовое слово для одного символа.</summary>
    private static void AddSingleChar(this List<byte> buffer, char character)
    {
        if (character <= 127)
        {
            buffer.Add((byte)(character + 1));
        }
        else
        {
            buffer.AddRange([EXTENDED_ASCII, (byte)(character - 127)]);
        }
    }

    #endregion

    #region SymbolSize

    /// <summary>Таблица поддерживаемых размеров символов ECC200.</summary>
    private static readonly List<MatrixSize> _supportedSizes =
    [
        (10, 10, RegionCounts.One, RegionCounts.One, 5, 1),
        (12, 12, RegionCounts.One, RegionCounts.One, 7, 1),
        (14, 14, RegionCounts.One, RegionCounts.One, 10, 1),
        (16, 16, RegionCounts.One, RegionCounts.One, 12, 1),
        (18, 18, RegionCounts.One, RegionCounts.One, 14, 1),
        (20, 20, RegionCounts.One, RegionCounts.One, 18, 1),
        (22, 22, RegionCounts.One, RegionCounts.One, 20, 1),
        (24, 24, RegionCounts.One, RegionCounts.One, 24, 1),
        (26, 26, RegionCounts.One, RegionCounts.One, 28, 1),
        (32, 32, RegionCounts.Two, RegionCounts.Two, 36, 1),
        (36, 36, RegionCounts.Two, RegionCounts.Two, 42, 1),
        (40, 40, RegionCounts.Two, RegionCounts.Two, 48, 1),
        (44, 44, RegionCounts.Two, RegionCounts.Two, 56, 1),
        (48, 48, RegionCounts.Two, RegionCounts.Two, 68, 1),
        (52, 52, RegionCounts.Two, RegionCounts.Two, 84, 2),
        (64, 64, RegionCounts.For, RegionCounts.For, 112, 2),
        (72, 72, RegionCounts.For, RegionCounts.For, 144, 4),
        (80, 80, RegionCounts.For, RegionCounts.For, 192, 4),
        (88, 88, RegionCounts.For, RegionCounts.For, 224, 4),
        (96, 96, RegionCounts.For, RegionCounts.For, 272, 4),
        (104, 104, RegionCounts.For, RegionCounts.For, 336, 6),
        (120, 120, RegionCounts.Six, RegionCounts.Six, 408, 6),
        (132, 132, RegionCounts.Six, RegionCounts.Six, 496, 8),
        (144, 144, RegionCounts.Six, RegionCounts.Six, 620, 10),
        (8, 18, RegionCounts.One, RegionCounts.One, 7, 1),
        (8, 32, RegionCounts.Two, RegionCounts.One, 11, 1),
        (12, 26, RegionCounts.One, RegionCounts.One, 14, 1),
        (12, 36, RegionCounts.Two, RegionCounts.One, 18, 1),
        (16, 36, RegionCounts.Two, RegionCounts.One, 24, 1),
        (16, 48, RegionCounts.Two, RegionCounts.One, 28, 1),
    ];

    /// <summary>Первое слово-заполнитель.</summary>
    private const byte RANDOMIZE_PAD = 129;

    /// <summary>Подбирает размер символа под число кодовых слов.</summary>
    private static (MatrixSize Symbol, int Capacity) SelectSymbolSize(int? rows, int? columns, int codewordCount)
    {
        IEnumerable<MatrixSize> candidates = _supportedSizes;

        if (rows.HasValue)
            candidates = candidates.Where(x => x.Rows == rows.Value);

        if (columns.HasValue)
            candidates = candidates.Where(x => x.Columns == columns.Value);

        /// <summary>Неизвестный размер не отклоняем — ищем по всей таблице.</summary>
        if (!candidates.Any())
            candidates = _supportedSizes;

        var candidatesWithCapacity = candidates.Select(x => (Symbol: x, Capacity: CalculateDataCapacity(x)));
        var match = candidatesWithCapacity.FirstOrDefault(x => x.Capacity >= codewordCount);

        /// <summary>Если данные не влезают — берём самый вместительный символ.</summary>
        return match.Symbol is not null ? match : candidatesWithCapacity.MaxBy(x => x.Capacity);
    }

    /// <summary>Сколько слов данных помещается в символ.</summary>
    private static int CalculateDataCapacity(MatrixSize symbol)
        => symbol.GetDataCols() * symbol.GetDataRows() / 8 - symbol.EccCount;

    /// <summary>Дополняет кодовые слова до ёмкости символа.</summary>
    private static byte[] PadToDataCapacity(byte[] codewords, int capacity)
    {
        /// <summary>Лишние слова отбрасываем.</summary>
        if (codewords.Length > capacity)
            return codewords[..capacity];

        if (codewords.Length == capacity)
            return codewords;

        List<byte> buffer = [.. codewords];

        if (buffer.Count < capacity)
            buffer.Add(RANDOMIZE_PAD);

        while (buffer.Count < capacity)
        {
            var position = buffer.Count + 1;
            var value = 129 + (149 * position % 253) + 1;
            buffer.Add((byte)(value <= 254 ? value : value - 254));
        }

        return [.. buffer];
    }

    /// <summary>Высота одной области данных в модулях.</summary>
    private static int GetRegionRowHeight(this MatrixSize symbol)
        => (symbol.Rows - (int)symbol.VerticalRegions * 2) / (int)symbol.VerticalRegions;

    /// <summary>Ширина одной области данных в модулях.</summary>
    private static int GetRegionColWidth(this MatrixSize symbol)
        => (symbol.Columns - (int)symbol.HorizontalRegions * 2) / (int)symbol.HorizontalRegions;

    /// <summary>Суммарная высота всех областей данных в модулях.</summary>
    private static int GetDataRows(this MatrixSize symbol)
        => symbol.GetRegionRowHeight() * (int)symbol.VerticalRegions;

    /// <summary>Суммарная ширина всех областей данных в модулях.</summary>
    private static int GetDataCols(this MatrixSize symbol)
        => symbol.GetRegionColWidth() * (int)symbol.HorizontalRegions;

    /// <summary>Слов данных в одном блоке коррекции ошибок.</summary>
    private static int GetDataCodewordsPerBlock(this MatrixSize symbol, int blockIndex)
        => symbol.Rows == 144 && symbol.Columns == 144
            ? blockIndex < 8 ? 156 : 155
            : CalculateDataCapacity(symbol) / symbol.BlockCount;

    /// <summary>Контрольных слов в одном блоке коррекции.</summary>
    private static int GetEccCodewordsPerBlock(this MatrixSize symbol)
        => symbol.EccCount / symbol.BlockCount;

    #endregion

    #region ErrorCorrection

    /// <summary>Размер поля Галлуа.</summary>
    private const int GaloisFieldSize = 256;

    /// <summary>Смещение в таблице степеней.</summary>
    private const int ExpTableOffset = 1;

    /// <summary>Таблица степеней двойки в поле Галлуа.</summary>
    private static readonly byte[] _expTable =
    [
        1, 2, 4, 8, 16, 32, 64, 128, 45, 90, 180, 69, 138, 57, 114, 228, 229, 231, 227, 235,
        251, 219, 155, 27, 54, 108, 216, 157, 23, 46, 92, 184, 93, 186, 89, 178, 73, 146, 9, 18,
        36, 72, 144, 13, 26, 52, 104, 208, 141, 55, 110, 220, 149, 7, 14, 28, 56, 112, 224, 237,
        247, 195, 171, 123, 246, 193, 175, 115, 230, 225, 239, 243, 203, 187, 91, 182, 65, 130, 41, 82,
        164, 101, 202, 185, 95, 190, 81, 162, 105, 210, 137, 63, 126, 252, 213, 135, 35, 70, 140, 53,
        106, 212, 133, 39, 78, 156, 21, 42, 84, 168, 125, 250, 217, 159, 19, 38, 76, 152, 29, 58,
        116, 232, 253, 215, 131, 43, 86, 172, 117, 234, 249, 223, 147, 11, 22, 44, 88, 176, 77, 154,
        25, 50, 100, 200, 189, 87, 174, 113, 226, 233, 255, 211, 139, 59, 118, 236, 245, 199, 163, 107,
        214, 129, 47, 94, 188, 85, 170, 121, 242, 201, 191, 83, 166, 97, 194, 169, 127, 254, 209, 143,
        51, 102, 204, 181, 71, 142, 49, 98, 196, 165, 103, 206, 177, 79, 158, 17, 34, 68, 136, 61,
        122, 244, 197, 167, 99, 198, 161, 111, 222, 145, 15, 30, 60, 120, 240, 205, 183, 67, 134, 33,
        66, 132, 37, 74, 148, 5, 10, 20, 40, 80, 160, 109, 218, 153, 31, 62, 124, 248, 221, 151,
        3, 6, 12, 24, 48, 96, 192, 173, 119, 238, 241, 207, 179, 75, 150, 1,
    ];

    /// <summary>Обратная таблица степеней.</summary>
    private static readonly byte[] _logTable =
    [
        0, 255, 1, 240, 2, 225, 241, 53, 3, 38, 226, 133, 242, 43, 54, 210, 4, 195, 39,
        114, 227, 106, 134, 28, 243, 140, 44, 23, 55, 118, 211, 234, 5, 219, 196, 96, 40, 222,
        115, 103, 228, 78, 107, 125, 135, 8, 29, 162, 244, 186, 141, 180, 45, 99, 24, 49, 56,
        13, 119, 153, 212, 199, 235, 91, 6, 76, 220, 217, 197, 11, 97, 184, 41, 36, 223, 253,
        116, 138, 104, 193, 229, 86, 79, 171, 108, 165, 126, 145, 136, 34, 9, 74, 30, 32, 163,
        84, 245, 173, 187, 204, 142, 81, 181, 190, 46, 88, 100, 159, 25, 231, 50, 207, 57, 147,
        14, 67, 120, 128, 154, 248, 213, 167, 200, 63, 236, 110, 92, 176, 7, 161, 77, 124, 221,
        102, 218, 95, 198, 90, 12, 152, 98, 48, 185, 179, 42, 209, 37, 132, 224, 52, 254, 239,
        117, 233, 139, 22, 105, 27, 194, 113, 230, 206, 87, 158, 80, 189, 172, 203, 109, 175, 166,
        62, 127, 247, 146, 66, 137, 192, 35, 252, 10, 183, 75, 216, 31, 83, 33, 73, 164, 144,
        85, 170, 246, 65, 174, 61, 188, 202, 205, 157, 143, 169, 82, 72, 182, 215, 191, 251, 47,
        178, 89, 151, 101, 94, 160, 123, 26, 112, 232, 21, 51, 238, 208, 131, 58, 69, 148, 18,
        15, 16, 68, 17, 121, 149, 129, 19, 155, 59, 249, 70, 214, 250, 168, 71, 201, 156, 64,
        60, 237, 130, 111, 20, 93, 122, 177, 150
    ];

    /// <summary>Добавляет контрольные слова и перемешивает блоки.</summary>
    private static byte[] AddErrorCorrection(byte[] data, MatrixSize symbol)
    {
        List<GaoisFieldPolynome> generatorCache = [new([1])];
        var dataLength = data.Length;
        var result = new byte[dataLength + symbol.EccCount];

        Array.Copy(data, result, dataLength);

        for (var block = 0; block < symbol.BlockCount; block++)
        {
            int blockLength = symbol.GetDataCodewordsPerBlock(block);
            int eccLength = symbol.GetEccCodewordsPerBlock();
            int[] blockData = new int[blockLength];
            var word = 0;

            for (var i = block; i < dataLength; i += symbol.BlockCount)
            {
                blockData[word++] = result[i];
            }

            int[] ecc = ComputeErrorCorrectionCodewords(blockData, eccLength, generatorCache);

            word = 0;
            for (var i = block; i < eccLength * symbol.BlockCount; i += symbol.BlockCount)
            {
                result[dataLength + i] = (byte)ecc[word++];
            }
        }

        return result;
    }

    /// <summary>Считает контрольные слова одного блока.</summary>
    private static int[] ComputeErrorCorrectionCodewords(int[] data, int eccLength, List<GaoisFieldPolynome> generatorCache)
    {
        GaoisFieldPolynome generator = GenerateGeneratorPolynomial(eccLength, generatorCache);
        GaoisFieldPolynome message = new(data);
        message = message.MultiplyByMonomial(eccLength, 1);

        GaoisFieldPolynome remainder = message.Divide(generator);

        var result = new int[eccLength];
        var offset = eccLength - remainder.Coefficients.Length;
        Array.Copy(remainder.Coefficients, 0, result, offset, remainder.Coefficients.Length);

        return result;
    }

    /// <summary>Возвращает порождающий многочлен нужной степени.</summary>
    private static GaoisFieldPolynome GenerateGeneratorPolynomial(int degree, List<GaoisFieldPolynome> cache)
    {
        if (degree >= cache.Count)
        {
            for (var d = cache.Count; d <= degree; d++)
            {
                cache.Add(cache[^1].Multiply(new([1, _expTable[d - 1 + ExpTableOffset]])));
            }
        }

        return cache[degree];
    }

    /// <summary>Умножает два элемента в поле Галлуа.</summary>
    private static int MultiplyInGaloisField(int left, int right)
    {
        if (left == 0 || right == 0)
            return 0;

        return _expTable[(_logTable[left] + _logTable[right]) % (GaloisFieldSize - 1)];
    }

    /// <summary>Степень многочлена.</summary>
    private static int Degree(this GaoisFieldPolynome polynomial)
        => polynomial.Coefficients.Length - 1;

    /// <summary>Проверяет, что многочлен нулевой.</summary>
    private static bool IsZero(this GaoisFieldPolynome polynomial)
        => polynomial.Coefficients[0] == 0;

    /// <summary>Складывает два многочлена через XOR.</summary>
    private static GaoisFieldPolynome Add(this GaoisFieldPolynome left, GaoisFieldPolynome right)
    {
        if (left.IsZero())
            return right;

        if (right.IsZero())
            return left;

        int[] leftCoefficients = left.Coefficients;
        int[] rightCoefficients = right.Coefficients;

        if (leftCoefficients.Length > rightCoefficients.Length)
        {
            (rightCoefficients, leftCoefficients) = (leftCoefficients, rightCoefficients);
        }

        var result = new int[rightCoefficients.Length];
        var offset = rightCoefficients.Length - leftCoefficients.Length;
        Array.Copy(rightCoefficients, result, offset);

        for (var i = offset; i < rightCoefficients.Length; i++)
        {
            result[i] = leftCoefficients[i - offset] ^ rightCoefficients[i];
        }

        return new(result);
    }

    /// <summary>Умножает многочлен на одночлен.</summary>
    private static GaoisFieldPolynome MultiplyByMonomial(this GaoisFieldPolynome polynomial, int degree, int scalar)
    {
        if (scalar == 0)
            return 0;

        var length = polynomial.Coefficients.Length;
        var result = new int[length + degree];

        for (var i = 0; i < length; i++)
        {
            result[i] = MultiplyInGaloisField(polynomial.Coefficients[i], scalar);
        }

        return new(result);
    }

    /// <summary>Перемножает два многочлена.</summary>
    private static GaoisFieldPolynome Multiply(this GaoisFieldPolynome left, GaoisFieldPolynome right)
    {
        if (left.IsZero() || right.IsZero())
            return 0;

        var result = new int[left.Coefficients.Length + right.Coefficients.Length - 1];

        for (var i = 0; i < left.Coefficients.Length; i++)
        {
            var leftCoefficient = left.Coefficients[i];

            for (var j = 0; j < right.Coefficients.Length; j++)
            {
                result[i + j] ^= MultiplyInGaloisField(leftCoefficient, right.Coefficients[j]);
            }
        }

        return new(result);
    }

    /// <summary>Делит многочлены и возвращает остаток.</summary>
    private static GaoisFieldPolynome Divide(this GaoisFieldPolynome dividend, GaoisFieldPolynome divisor)
    {
        var value = dividend;
        /// <summary>Старший коэффициент делителя лежит под индексом 0.</summary>
        var divisorLeading = divisor.Coefficients[0];
        var divisorInverse = _expTable[GaloisFieldSize - 1 - _logTable[divisorLeading]];

        while (value.Degree() >= divisor.Degree() && !value.IsZero())
        {
            var degreeDifference = value.Degree() - divisor.Degree();
            var factor = MultiplyInGaloisField(value.Coefficients[0], divisorInverse);
            value = value.Add(divisor.MultiplyByMonomial(degreeDifference, factor));
        }

        return value;
    }

    #endregion

    #region ModulePlacement

    /// <summary>Собирает битовую карту модулей символа.</summary>
    private static uint[] BuildModuleBitmap(byte[] codewords, MatrixSize symbol)
    {
        int moduleCount = symbol.GetDataCols() * symbol.GetDataRows();

        uint[] occupied = AllocateBitBuffer(moduleCount);
        uint[] data = AllocateBitBuffer(moduleCount);

        PlaceCodewords(symbol, occupied, data, codewords);

        return ComposeSymbol(symbol, data);
    }

    /// <summary>Создаёт буфер на нужное число бит.</summary>
    private static uint[] AllocateBitBuffer(int bitCount)
    {
        var remainder = bitCount % 32 != 0 ? 1 : 0;

        return new uint[bitCount / 32 + remainder];
    }

    /// <summary>Устанавливает бит по индексу.</summary>
    private static void SetBit(this uint[] buffer, int index, bool value)
    {
        var word = index / 32;
        var shift = 31 - (index % 32);

        if (value)
            buffer[word] |= (uint)1 << shift;
        else
            buffer[word] &= ~((uint)1 << shift);
    }

    /// <summary>Читает бит по индексу.</summary>
    private static bool GetBit(this uint[] buffer, int index)
    {
        var word = index / 32;
        var shift = 31 - (index % 32);

        return ((buffer[word] >> shift) & 1) == 1;
    }

    /// <summary>Размещает кодовые слова зигзагом.</summary>
    private static void PlaceCodewords(MatrixSize symbol, uint[] occupied, uint[] data, byte[] codewords)
    {
        var position = 0;
        var row = 4;
        var column = 0;

        while (row < symbol.GetDataRows() || column < symbol.GetDataCols())
        {
            if (row == symbol.GetDataRows() && column == 0)
            {
                PlaceCorner1(symbol, occupied, data, codewords[position++]);
            }

            if (row == symbol.GetDataRows() - 2 && column == 0 && (symbol.GetDataCols() % 4) != 0)
            {
                PlaceCorner2(symbol, occupied, data, codewords[position++]);
            }

            if (row == symbol.GetDataRows() - 2 && column == 0 && (symbol.GetDataCols() % 8) == 4)
            {
                PlaceCorner3(symbol, occupied, data, codewords[position++]);
            }

            if (row == symbol.GetDataRows() + 4 && column == 2 && (symbol.GetDataCols() % 8) == 0)
            {
                PlaceCorner4(symbol, occupied, data, codewords[position++]);
            }

            /// <summary>Проход вверх и вправо.</summary>
            while (true)
            {
                if (row < symbol.GetDataRows() && column >= 0 && !IsModuleOccupied(symbol, occupied, row, column))
                {
                    PlaceUtahCodeword(symbol, occupied, data, row, column, codewords[position++]);
                }

                row -= 2;
                column += 2;

                if (row < 0 || column >= symbol.GetDataCols())
                    break;
            }

            row += 1;
            column += 3;

            /// <summary>Проход вниз и влево.</summary>
            while (true)
            {
                if (row >= 0 && column < symbol.GetDataCols() && !IsModuleOccupied(symbol, occupied, row, column))
                {
                    PlaceUtahCodeword(symbol, occupied, data, row, column, codewords[position++]);
                }

                row += 2;
                column -= 2;

                if (row >= symbol.GetDataRows() || column < 0)
                    break;
            }

            row += 3;
            column += 1;
        }

        /// <summary>Заполняет нижний правый угол.</summary>
        if (!IsModuleOccupied(symbol, occupied, symbol.GetDataRows() - 1, symbol.GetDataCols() - 1))
        {
            PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 1, symbol.GetDataCols() - 1, 255, 0);
            PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 2, symbol.GetDataCols() - 2, 255, 0);
        }

        /// <summary>Проход должен разместить все кодовые слова.</summary>
        Debug.Assert(
            position == codewords.Length,
            $"Размещено {position} кодовых слов из {codewords.Length} для символа {symbol.Rows}x{symbol.Columns}");
    }

    /// <summary>Проверяет, занят ли модуль.</summary>
    private static bool IsModuleOccupied(MatrixSize symbol, uint[] occupied, int row, int column)
        => occupied.GetBit(column + row * symbol.GetDataCols());

    /// <summary>Ставит один бит кодового слова в модуль.</summary>
    private static void PlaceModule(MatrixSize symbol, uint[] occupied, uint[] data, int row, int column, byte codeword, int bitIndex)
    {
        if (row < 0)
        {
            row += symbol.GetDataRows();
            column += 4 - ((symbol.GetDataRows() + 4) % 8);
        }

        if (column < 0)
        {
            column += symbol.GetDataCols();
            row += 4 - ((symbol.GetDataCols() + 4) % 8);
        }

        /// <summary>Модуль не должен быть уже занят.</summary>
        Debug.Assert(!IsModuleOccupied(symbol, occupied, row, column), $"Модуль занят: строка {row}, столбец {column}");

        occupied.SetBit(column + row * symbol.GetDataCols(), true);

        var isSet = ((codeword >> (7 - bitIndex)) & 1) == 1;
        data.SetBit(column + row * symbol.GetDataCols(), isSet);
    }

    /// <summary>Ставит 8 бит слова в блок 2x4.</summary>
    private static void PlaceUtahCodeword(MatrixSize symbol, uint[] occupied, uint[] data, int row, int column, byte codeword)
    {
        PlaceModule(symbol, occupied, data, row - 2, column - 2, codeword, 0);
        PlaceModule(symbol, occupied, data, row - 2, column - 1, codeword, 1);
        PlaceModule(symbol, occupied, data, row - 1, column - 2, codeword, 2);
        PlaceModule(symbol, occupied, data, row - 1, column - 1, codeword, 3);
        PlaceModule(symbol, occupied, data, row - 1, column - 0, codeword, 4);
        PlaceModule(symbol, occupied, data, row - 0, column - 2, codeword, 5);
        PlaceModule(symbol, occupied, data, row - 0, column - 1, codeword, 6);
        PlaceModule(symbol, occupied, data, row - 0, column - 0, codeword, 7);
    }

    /// <summary>Угловой случай 1.</summary>
    private static void PlaceCorner1(MatrixSize symbol, uint[] occupied, uint[] data, byte codeword)
    {
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 1, 0, codeword, 0);
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 1, 1, codeword, 1);
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 1, 2, codeword, 2);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 2, codeword, 3);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 1, codeword, 4);
        PlaceModule(symbol, occupied, data, 1, symbol.GetDataCols() - 1, codeword, 5);
        PlaceModule(symbol, occupied, data, 2, symbol.GetDataCols() - 1, codeword, 6);
        PlaceModule(symbol, occupied, data, 3, symbol.GetDataCols() - 1, codeword, 7);
    }

    /// <summary>Угловой случай 2.</summary>
    private static void PlaceCorner2(MatrixSize symbol, uint[] occupied, uint[] data, byte codeword)
    {
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 3, 0, codeword, 0);
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 2, 0, codeword, 1);
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 1, 0, codeword, 2);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 4, codeword, 3);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 3, codeword, 4);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 2, codeword, 5);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 1, codeword, 6);
        PlaceModule(symbol, occupied, data, 1, symbol.GetDataCols() - 1, codeword, 7);
    }

    /// <summary>Угловой случай 3.</summary>
    private static void PlaceCorner3(MatrixSize symbol, uint[] occupied, uint[] data, byte codeword)
    {
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 3, 0, codeword, 0);
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 2, 0, codeword, 1);
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 1, 0, codeword, 2);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 2, codeword, 3);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 1, codeword, 4);
        PlaceModule(symbol, occupied, data, 1, symbol.GetDataCols() - 1, codeword, 5);
        PlaceModule(symbol, occupied, data, 2, symbol.GetDataCols() - 1, codeword, 6);
        PlaceModule(symbol, occupied, data, 3, symbol.GetDataCols() - 1, codeword, 7);
    }

    /// <summary>Угловой случай 4.</summary>
    private static void PlaceCorner4(MatrixSize symbol, uint[] occupied, uint[] data, byte codeword)
    {
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 1, 0, codeword, 0);
        PlaceModule(symbol, occupied, data, symbol.GetDataRows() - 1, symbol.GetDataCols() - 1, codeword, 1);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 3, codeword, 2);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 2, codeword, 3);
        PlaceModule(symbol, occupied, data, 0, symbol.GetDataCols() - 1, codeword, 4);
        PlaceModule(symbol, occupied, data, 1, symbol.GetDataCols() - 3, codeword, 5);
        PlaceModule(symbol, occupied, data, 1, symbol.GetDataCols() - 2, codeword, 6);
        PlaceModule(symbol, occupied, data, 1, symbol.GetDataCols() - 1, codeword, 7);
    }

    /// <summary>Добавляет служебные полосы и собирает символ.</summary>
    private static uint[] ComposeSymbol(MatrixSize symbol, uint[] data)
    {
        uint[] result = new uint[symbol.Rows * symbol.Columns];

        result.PlaceTopClockTrack(symbol);
        result.PlaceRightClockTrack(symbol);
        result.PlaceBottomBorder(symbol);
        result.PlaceLeftBorder(symbol);
        result.CopyDataRegions(symbol, data);

        return result;
    }

    /// <summary>Переносит данные из областей в общий символ.</summary>
    private static void CopyDataRegions(this uint[] symbol, MatrixSize info, uint[] data)
    {
        for (var regionColumn = 0; regionColumn < (int)info.HorizontalRegions; regionColumn++)
        {
            for (var regionRow = 0; regionRow < (int)info.VerticalRegions; regionRow++)
            {
                for (var x = 0; x < info.GetRegionColWidth(); x++)
                {
                    int dataColumn = info.GetRegionColWidth() * regionColumn + x;
                    int symbolColumn = (2 + info.GetRegionColWidth()) * regionColumn + x + 1;

                    for (var y = 0; y < info.GetRegionRowHeight(); y++)
                    {
                        int dataRow = info.GetRegionRowHeight() * regionRow + y;
                        int symbolRow = (2 + info.GetRegionRowHeight()) * regionRow + y + 1;

                        bool isSet = data.GetBit(dataColumn + dataRow * info.GetDataCols());

                        symbol.SetBit(symbolColumn * info.Rows + symbolRow, isSet);
                    }
                }
            }
        }
    }

    /// <summary>Верхняя полоса синхронизации.</summary>
    private static void PlaceTopClockTrack(this uint[] symbol, MatrixSize info)
    {
        for (var row = 0; row < info.Rows; row += info.GetRegionRowHeight() + 2)
        {
            for (var column = 0; column < info.Columns; column += 2)
            {
                symbol.SetBit(column * info.Rows + row, true);
            }
        }
    }

    /// <summary>Правая полоса синхронизации.</summary>
    private static void PlaceRightClockTrack(this uint[] symbol, MatrixSize info)
    {
        for (var column = info.GetRegionColWidth() + 1; column < info.Columns; column += info.GetRegionColWidth() + 2)
        {
            for (var row = 1; row < info.Rows; row += 2)
            {
                symbol.SetBit(column * info.Rows + row, true);
            }
        }
    }

    /// <summary>Нижняя граница символа.</summary>
    private static void PlaceBottomBorder(this uint[] symbol, MatrixSize info)
    {
        for (var row = info.GetRegionRowHeight() + 1; row < info.Rows; row += info.GetRegionRowHeight() + 2)
        {
            for (var column = 0; column < info.Columns; column++)
            {
                symbol.SetBit(column * info.Rows + row, true);
            }
        }
    }

    /// <summary>Левая граница символа.</summary>
    private static void PlaceLeftBorder(this uint[] symbol, MatrixSize info)
    {
        for (var column = 0; column < info.Columns; column += info.GetRegionColWidth() + 2)
        {
            for (var row = 0; row < info.Rows; row++)
            {
                symbol.SetBit(column * info.Rows + row, true);
            }
        }
    }

    #endregion

    #region ModuleMatrix

    /// <summary>Заполненный модуль символа.</summary>
    private const byte ACTIVE = 1;

    /// <summary>Пустой модуль символа.</summary>
    private const byte EMPTY = 0;

    /// <summary>Ширина пустой рамки вокруг символа.</summary>
    private const int BORDER = 2;

    /// <summary>Символы отрисовки: по половине модуля сверху и снизу.</summary>
    private const string CharTemplate = " ▄▀█";

    /// <summary>Символы отрисовки для инвертированного вида.</summary>
    private const string InvertedCharTemplate = "█▀▄ ";

    /// <summary>Переводит символ в матрицу значений с рамкой.</summary>
    private static List<byte[]> CreateModuleMatrix(uint[] moduleBitmap, MatrixSize symbol)
    {
        var matrix = CreateFilledMatrix(GetPaddedMatrixSize(Math.Max(symbol.Columns, symbol.Rows)));

        for (var column = 0; column < symbol.Columns; column++)
        {
            for (var row = 0; row < symbol.Rows; row++)
            {
                matrix[column + BORDER][row + BORDER] = GetModule(moduleBitmap, symbol, row, column) ? EMPTY : ACTIVE;
            }
        }

        return matrix;
    }

    /// <summary>Читает модуль символа по строке и столбцу.</summary>
    private static bool GetModule(uint[] moduleBitmap, MatrixSize symbol, int row, int column)
        => moduleBitmap.GetBit(column * symbol.Rows + row);

    /// <summary>Размер матрицы с учётом пустой рамки вокруг символа.</summary>
    private static int GetPaddedMatrixSize(int size)
        => size + BORDER * 2;

    /// <summary>Создаёт квадратную матрицу, заполненную значением.</summary>
    private static List<byte[]> CreateFilledMatrix(int size, byte value = ACTIVE)
    {
        List<byte[]> matrix = [];

        for (var i = 0; i < size; i++)
        {
            matrix.Add(new byte[size]);
        }

        return matrix.Fill(value);
    }

    /// <summary>Заполняет все элементы матрицы значением.</summary>
    private static List<byte[]> Fill(this List<byte[]> matrix, byte value)
    {
        for (var row = 0; row < matrix.Count; row++)
        {
            for (var column = 0; column < matrix.Count; column++)
            {
                matrix[row][column] = value;
            }
        }

        return matrix;
    }

    #endregion

    #region Rendering

    /// <summary>Преобразует матрицу в строку символов.</summary>
    private static string ToMatrixString(this List<byte[]> matrix, bool invert)
    {
        var builder = new StringBuilder();

        builder.AppendMatrix(matrix, invert);

        return builder.ToString();
    }

    /// <summary>Дописывает строки матрицы по две в одну строку.</summary>
    private static void AppendMatrix(this StringBuilder builder, List<byte[]> matrix, bool invert)
    {
        var pairedRows = matrix.Count % 2 == 1 ? matrix.Count + 1 : matrix.Count;

        /// <summary>Матрица адресуется по столбцам, а выводится по строкам.</summary>
        for (var row = 0; row < pairedRows; row += 2)
        {
            for (var column = 0; column < matrix.Count; column++)
            {
                var top = matrix[column][row];
                var bottom = row < matrix.Count - 1 ? matrix[column][row + 1] : ACTIVE;

                builder.Append(invert ? GetInvertedChar(top, bottom) : GetChar(top, bottom));
            }

            builder.AppendLine();
        }
    }

    /// <summary>Символ отрисовки для пары модулей.</summary>
    private static char GetChar(byte top, byte bottom)
    {
        Debug.Assert(top <= ACTIVE && bottom <= ACTIVE, "Модуль может быть только заполненным или пустым");

        return CharTemplate[(top << 1) | bottom];
    }

    /// <summary>Символ отрисовки для пары модулей навыворот.</summary>
    private static char GetInvertedChar(byte top, byte bottom)
    {
        Debug.Assert(top <= ACTIVE && bottom <= ACTIVE, "Модуль может быть только заполненным или пустым");

        return InvertedCharTemplate[(top << 1) | bottom];
    }

    /// <summary>Центрирует текст под кодом.</summary>
    public static string CenterText(string code, string text)
    {
        var lines = code.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var width = lines.Length == 0 ? 0 : lines.Max(line => line.TrimEnd('\r').Length);

        var builder = new StringBuilder();

        foreach (string part in SplitByWidth(text, width))
        {
            int indent = Math.Max(width - part.TrimEnd('\r').Length, 0) / 2;

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(' ', indent).Append(part.TrimEnd('\r'));
        }

        return builder.ToString();
    }

    /// <summary>Делит текст на части по ширине.</summary>
    private static IEnumerable<string> SplitByWidth(string text, int width)
    {
        if (width <= 0 || text.Length <= width)
        {
            return [text];
        }

        var parts = new List<string>();

        for (var offset = 0; offset < text.Length; offset += width)
        {
            int length = Math.Min(width, text.Length - offset);

            /// <summary>Не разрываем суррогатную пару.</summary>
            if (offset + length < text.Length && char.IsHighSurrogate(text[offset + length - 1]))
            {
                length--;
            }

            parts.Add(text.Substring(offset, length));
        }

        return parts;
    }

    #endregion
}
