import React, { useState } from 'react';
import {
    Card,
    CardContent,
    Typography,
    TextField,
    Select,
    MenuItem,
    FormControl,
    InputLabel,
    Button,
    Box,
    Alert,
    CircularProgress,
    FormHelperText,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { ja } from 'date-fns/locale';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { Add } from '@mui/icons-material';
import { CreateTransactionDto, TransactionType } from '../types/transaction';

interface TransactionFormProps {
    onSubmit: (transaction: CreateTransactionDto) => Promise<void>;
    loading?: boolean;
}

// Validation schema
const transactionSchema = yup.object({
    amount: yup
        .number()
        .positive('金額は正の数値である必要があります')
        .required('金額は必須です')
        .typeError('有効な数値を入力してください'),
    description: yup
        .string()
        .required('説明は必須です')
        .max(200, '説明は200文字以内で入力してください')
        .trim(),
    type: yup
        .number()
        .oneOf([TransactionType.Income, TransactionType.Expense], '有効な取引タイプを選択してください')
        .required('取引タイプは必須です'),
    date: yup
        .date()
        .required('日付は必須です')
        .typeError('有効な日付を入力してください'),
});

type FormData = yup.InferType<typeof transactionSchema>;

export const TransactionForm: React.FC<TransactionFormProps> = ({
    onSubmit,
    loading = false,
}) => {
    const [submitError, setSubmitError] = useState<string | null>(null);

    const {
        control,
        handleSubmit,
        reset,
        formState: { errors, isSubmitting },
    } = useForm<FormData>({
        resolver: yupResolver(transactionSchema),
        defaultValues: {
            amount: undefined,
            description: '',
            type: TransactionType.Expense,
            date: new Date(),
        },
    });

    const handleFormSubmit = async (data: FormData) => {
        try {
            setSubmitError(null);

            // Convert form data to DTO format
            const transactionDto: CreateTransactionDto = {
                amount: data.amount!,
                description: data.description.trim(),
                type: data.type,
                date: data.date.toISOString(),
            };

            await onSubmit(transactionDto);

            // Clear form after successful submission
            reset({
                amount: undefined,
                description: '',
                type: TransactionType.Expense,
                date: new Date(),
            });
        } catch (error) {
            setSubmitError(
                error instanceof Error
                    ? error.message
                    : '取引の作成中にエラーが発生しました'
            );
        }
    };

    const isFormLoading = loading || isSubmitting;

    return (
        <Card>
            <CardContent>
                <Typography variant="h6" component="h2" gutterBottom>
                    新しい取引を追加
                </Typography>

                {submitError && (
                    <Alert severity="error" sx={{ mb: 2 }}>
                        {submitError}
                    </Alert>
                )}

                <Box
                    component="form"
                    onSubmit={handleSubmit(handleFormSubmit)}
                    noValidate
                >
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        {/* Amount Field */}
                        <Controller
                            name="amount"
                            control={control}
                            render={({ field }) => (
                                <TextField
                                    {...field}
                                    label="金額"
                                    type="number"
                                    fullWidth
                                    error={!!errors.amount}
                                    helperText={errors.amount?.message}
                                    disabled={isFormLoading}
                                    inputProps={{
                                        min: 0,
                                        step: 1,
                                    }}
                                    InputProps={{
                                        startAdornment: <Typography sx={{ mr: 1 }}>¥</Typography>,
                                    }}
                                />
                            )}
                        />

                        {/* Description Field */}
                        <Controller
                            name="description"
                            control={control}
                            render={({ field }) => (
                                <TextField
                                    {...field}
                                    label="説明"
                                    fullWidth
                                    multiline
                                    rows={2}
                                    error={!!errors.description}
                                    helperText={errors.description?.message}
                                    disabled={isFormLoading}
                                    placeholder="例: 食費、給与、交通費など"
                                />
                            )}
                        />

                        {/* Transaction Type Field */}
                        <Controller
                            name="type"
                            control={control}
                            render={({ field }) => (
                                <FormControl fullWidth error={!!errors.type} disabled={isFormLoading}>
                                    <InputLabel>取引タイプ</InputLabel>
                                    <Select
                                        {...field}
                                        label="取引タイプ"
                                    >
                                        <MenuItem value={TransactionType.Income}>
                                            収入
                                        </MenuItem>
                                        <MenuItem value={TransactionType.Expense}>
                                            支出
                                        </MenuItem>
                                    </Select>
                                    {errors.type && (
                                        <FormHelperText>{errors.type.message}</FormHelperText>
                                    )}
                                </FormControl>
                            )}
                        />

                        {/* Date Field */}
                        <Controller
                            name="date"
                            control={control}
                            render={({ field }) => (
                                <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={ja}>
                                    <DatePicker
                                        {...field}
                                        label="日付"
                                        disabled={isFormLoading}
                                        slotProps={{
                                            textField: {
                                                fullWidth: true,
                                                error: !!errors.date,
                                                helperText: errors.date?.message,
                                            },
                                        }}
                                    />
                                </LocalizationProvider>
                            )}
                        />

                        {/* Submit Button */}
                        <Button
                            type="submit"
                            variant="contained"
                            size="large"
                            disabled={isFormLoading}
                            startIcon={
                                isFormLoading ? (
                                    <CircularProgress size={20} />
                                ) : (
                                    <Add />
                                )
                            }
                            sx={{ mt: 1 }}
                        >
                            {isFormLoading ? '追加中...' : '取引を追加'}
                        </Button>
                    </Box>
                </Box>
            </CardContent>
        </Card>
    );
};